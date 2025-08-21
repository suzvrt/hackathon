using System.Threading.Channels;
using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;
using hackathon.Infrastructure.Telemetry;

namespace hackathon.Infrastructure.Services;

public class TelemetriaService : ITelemetriaService
{
    private readonly Channel<TelemetriaMessage> _channel;
    private readonly ITelemetriaRepository _repository;

    public TelemetriaService(
        Channel<TelemetriaMessage> channel,
        ITelemetriaRepository repository)
    {
        _channel = channel;
        _repository = repository;
    }

    public void RegistrarRequisicao(string nomeEndpoint, TimeSpan duracao, bool sucesso)
    {
        // não bloqueia o request; canal é unbounded (SingleReader, MultiWriter)
        _channel.Writer.TryWrite(new TelemetriaEvent(
            nomeEndpoint,
            (int)duracao.TotalMilliseconds,
            sucesso));
    }

    public async Task<TelemetriaResponse> ObterTelemetriaAsync(DateTime dataReferencia)
    {
        // 1) flush sob demanda para garantir que nada fique pendente em memória
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await _channel.Writer.WriteAsync(new TelemetriaFlush(tcs));
        await tcs.Task; // aguarda o worker confirmar o flush

        // 2) lê do repositório (estado persistido)
        var registros = await _repository.ObterTelemetriaPorDataAsync(dataReferencia);
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
}
