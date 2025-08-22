using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;
using hackathon.Infrastructure.Telemetria;

namespace hackathon.Infrastructure.Services;

public class TelemetriaService : ITelemetriaService
{
    private readonly Channel<TelemetriaMessage> _channel;
    private readonly IServiceProvider _serviceProvider;

    public TelemetriaService(
        Channel<TelemetriaMessage> channel,
        IServiceProvider serviceProvider)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
    }

    public async Task<TelemetriaResponse> ObterTelemetriaAsync(DateOnly dataReferencia)
    {
        // Flush sob demanda para garantir que nada fique pendente em memória
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await _channel.Writer.WriteAsync(new TelemetriaFlush(tcs));
        await tcs.Task; // Aguarda o worker confirmar o flush

        // Lê do repositório (estado persistido)
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITelemetriaRepository>();
        var registros = await repository.ObterTelemetriaPorDataAsync(dataReferencia);
        var dataFormatada = dataReferencia.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        return new TelemetriaResponse
        {
            DataReferencia = dataFormatada,
            ListaEndpoints = registros.Select(r => new EndpointTelemetria
            {
                NomeApi = r.NomeApi,
                QtdRequisicoes = r.QtdRequisicoes,
                TempoMedio = r.TempoMedio,
                TempoMinimo = r.TempoMinimo,
                TempoMaximo = r.TempoMaximo,
                PercentualSucesso = r.PercentualSucesso
            }).ToList()
        };
    }

    public void RegistrarRequisicao(string nomeEndpoint, TimeSpan duracao, bool sucesso)
    {
        var evento = new TelemetriaEvent
        {
            EndpointName = nomeEndpoint,
            DurationMs = duracao.TotalMilliseconds,
            StatusCode = sucesso ? 200 : 500,
            Timestamp = DateTime.UtcNow
        };

        // Write to channel asynchronously to avoid blocking
        _ = Task.Run(async () =>
        {
            try
            {
                await _channel.Writer.WriteAsync(new TelemetriaEventMessage(evento));
            }
            catch
            {
                // Falhar silenciosamente - telemetria não deve quebrar a aplicação principal
            }
        });
    }

    public async Task RegistrarRequisicaoAsync(string nomeEndpoint, TimeSpan duracao, bool sucesso)
    {
        var evento = new TelemetriaEvent
        {
            EndpointName = nomeEndpoint,
            DurationMs = duracao.TotalMilliseconds,
            StatusCode = sucesso ? 200 : 500,
            Timestamp = DateTime.UtcNow
        };

        await _channel.Writer.WriteAsync(new TelemetriaEventMessage(evento));
    }
}
