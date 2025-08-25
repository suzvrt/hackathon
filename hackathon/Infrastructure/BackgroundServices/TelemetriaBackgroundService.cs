using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Infrastructure.Telemetria;

namespace hackathon.Infrastructure.BackgroundServices;

public class TelemetriaBackgroundService : BackgroundService
{
    private readonly Channel<TelemetriaMessage> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetriaBackgroundService> _logger;

    private readonly Dictionary<string, Metrics> _agg = new(StringComparer.Ordinal);

    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(30);

    public TelemetriaBackgroundService(
        Channel<TelemetriaMessage> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<TelemetriaBackgroundService> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TelemetriaBackgroundService iniciando com loop híbrido. Intervalo de flush: {FlushInterval}", FlushInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Tenta ler o que já está no canal imediatamente
                while (_channel.Reader.TryRead(out var message))
                {
                    ProcessMessage(message);
                }

                // Espera de forma inteligente: acorda se uma nova mensagem chegar ou se o timeout for atingido
                using var cts = new CancellationTokenSource(FlushInterval);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cts.Token);

                await _channel.Reader.WaitToReadAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Timeout do FlushInterval atingido - comportamento esperado
            }
            catch (OperationCanceledException)
            {
                // A aplicação está sendo encerrada
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no loop principal do TelemetriaBackgroundService.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            // Sempre drenamos e fazemos o flush, seja por mensagem ou por timeout
            await DrainAndFlushAsync(stoppingToken);
        }

        _logger.LogInformation("Loop principal encerrado. Executando flush final.");
        await DrainAndFlushAsync(CancellationToken.None);
        _logger.LogInformation("TelemetriaBackgroundService encerrado.");
    }

    private async Task DrainAndFlushAsync(CancellationToken stoppingToken)
    {
        // Drena o canal: lê todas as mensagens pendentes
        int messagesProcessed = 0;
        while (_channel.Reader.TryRead(out var msg))
        {
            ProcessMessage(msg);
            messagesProcessed++;
        }

        if (messagesProcessed > 0)
        {
            _logger.LogInformation("{Count} novas mensagens de telemetria foram processadas.", messagesProcessed);
        }

        // Flush mais frequente se houver muitas mensagens para melhor performance
        if (messagesProcessed > 5)
        {
            _logger.LogInformation("Muitas mensagens processadas ({Count}), forçando flush imediato.", messagesProcessed);
            await FlushAsync(stoppingToken);
        }
        else
        {
            await FlushAsync(stoppingToken);
        }
    }

    private void ProcessMessage(TelemetriaEvent ev)
    {
        var durationMs = (int)ev.DurationMs;
        var isSuccess = ev.StatusCode >= 200 && ev.StatusCode <= 299;

        if (!_agg.TryGetValue(ev.EndpointName, out var m))
        {
            m = new Metrics();
            _agg[ev.EndpointName] = m;
            _logger.LogDebug("Novo endpoint registrado: {EndpointName}", ev.EndpointName);
        }

        m.TotalRequests++;
        if (isSuccess) m.SuccessfulRequests++;
        m.TotalDuration += durationMs;

        if (durationMs < m.MinDuration)
            m.MinDuration = durationMs;
        if (durationMs > m.MaxDuration)
            m.MaxDuration = durationMs;

        _logger.LogDebug("Mensagem processada: {EndpointName} - Duração: {Duration}ms, Sucesso: {Success}, Total: {Total}", 
            ev.EndpointName, durationMs, isSuccess, m.TotalRequests);
    }

    private void ProcessMessage(TelemetriaMessage message)
    {
        switch (message)
        {
            case TelemetriaEventMessage eventMsg:
                ProcessMessage(eventMsg.Payload);
                break;
            case TelemetriaFlush flushMsg:
                flushMsg.Tcs.SetResult(true);
                break;
        }
    }

    private async Task FlushAsync(CancellationToken ct)
    {
        if (_agg.Count == 0)
        {
            _logger.LogInformation("FlushAsync chamado, mas não há dados para persistir.");
            return;
        }

        _logger.LogInformation("Iniciando flush de {AggCount} registros agregados.", _agg.Count);
        var sw = Stopwatch.StartNew();

        var snapshot = new Dictionary<string, Metrics>(_agg, StringComparer.Ordinal);
        _agg.Clear();

        var hoje = DateTime.Today;
        var registros = new List<TelemetriaRecord>(snapshot.Count);
        foreach (var (endpoint, m) in snapshot)
        {
            registros.Add(new TelemetriaRecord
            {
                DataReferencia = hoje,
                NomeApi = endpoint,
                QtdRequisicoes = m.TotalRequests,
                TempoMedio = m.TotalRequests > 0 ? (int)(m.TotalDuration / m.TotalRequests) : 0,
                TempoMinimo = m.MinDuration,
                TempoMaximo = m.MaxDuration,
                PercentualSucesso = m.TotalRequests > 0 ? (decimal)m.SuccessfulRequests / m.TotalRequests : 0
            });
        }

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITelemetriaRepository>();

        const int maxRetries = 3;
        var delay = TimeSpan.FromMilliseconds(150);

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                _logger.LogDebug("Tentativa {Attempt}: Chamando repository.SalvarTelemetriaAsync com {RecordCount} registros.", attempt, registros.Count);

                var repoSw = Stopwatch.StartNew();
                await repository.SalvarTelemetriaAsync(registros);
                repoSw.Stop();

                _logger.LogInformation("repository.SalvarTelemetriaAsync concluído com sucesso na tentativa {Attempt} em {ElapsedMilliseconds}ms.", attempt, repoSw.ElapsedMilliseconds);
                break;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex,
                    "Falha ao salvar telemetria (tentativa {Attempt}/{MaxRetries}). Aguardando {Delay}...",
                    attempt, maxRetries, delay);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro final ao salvar telemetria após {MaxRetries} tentativas.", maxRetries);
                break;
            }
        }

        sw.Stop();
        _logger.LogInformation("FlushAsync concluído em {ElapsedMilliseconds}ms.", sw.ElapsedMilliseconds);
    }

    private sealed class Metrics
    {
        public int TotalRequests;
        public int SuccessfulRequests;
        public long TotalDuration;
        public int MinDuration;
        public int MaxDuration;
    }
}
