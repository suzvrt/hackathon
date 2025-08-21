using System.Threading.Channels;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Infrastructure.Telemetry;

namespace hackathon.Infrastructure.BackgroundServices;

public class TelemetriaBackgroundService : BackgroundService
{
    private readonly Channel<TelemetriaMessage> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetriaBackgroundService> _logger;

    // agregador em memória; acessado apenas pelo single-reader -> sem locks
    private readonly Dictionary<string, Metrics> _agg = new(StringComparer.Ordinal);

    // configuração simples
    private static readonly TimeSpan FlushInterval = TimeSpan.FromMinutes(1);
    private static readonly int MaxBatchSize = 2_000; // flush se acumular muitos eventos

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
        using var timer = new PeriodicTimer(FlushInterval);
        int eventsSinceLastFlush = 0;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Aguarda mensagem ou tick do timer (o que vier primeiro)
                var msgTask = _channel.Reader.ReadAsync(stoppingToken).AsTask();
                var tickTask = timer.WaitForNextTickAsync(stoppingToken).AsTask();

                var completed = await Task.WhenAny(msgTask, tickTask);

                if (completed == msgTask)
                {
                    var msg = await msgTask; // já concluída
                    switch (msg)
                    {
                        case TelemetriaEvent ev:
                            Aggregate(ev);
                            eventsSinceLastFlush++;
                            if (eventsSinceLastFlush >= MaxBatchSize)
                            {
                                await FlushAsync(stoppingToken);
                                eventsSinceLastFlush = 0;
                                // reinicia o timer (cria novo)
                                timer.Dispose();
                                // recria o timer para reiniciar o intervalo
                                // (evita flush imediato após batch flush)
                                // Nota: se preferir não reiniciar, remova este bloco.
                                // Apenas para manter "intervalo cheio" entre flushes.
                                // Recria:
                                // using var newTimer = new PeriodicTimer(FlushInterval); // não compila no escopo atual
                                //=> manter simples: não reiniciar; somente prossiga.
                            }
                            break;

                        case TelemetriaFlush f:
                            await FlushAsync(stoppingToken);
                            f.Tcs.TrySetResult(true);
                            eventsSinceLastFlush = 0;
                            break;
                    }
                }
                else
                {
                    // tick do timer: flush periódico
                    await FlushAsync(stoppingToken);
                    eventsSinceLastFlush = 0;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // flush final no shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no TelemetriaBackgroundService (loop principal).");
        }

        try
        {
            await FlushAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar flush final de telemetria.");
        }
    }

    private void Aggregate(TelemetriaEvent ev)
    {
        if (!_agg.TryGetValue(ev.Endpoint, out var m))
        {
            m = new Metrics();
            _agg[ev.Endpoint] = m;
        }

        m.TotalRequests++;
        if (ev.Success) m.SuccessfulRequests++;
        m.TotalDuration += ev.DurationMs;

        if (m.MinDuration == 0 || ev.DurationMs < m.MinDuration)
            m.MinDuration = ev.DurationMs;
        if (ev.DurationMs > m.MaxDuration)
            m.MaxDuration = ev.DurationMs;
    }

    private async Task FlushAsync(CancellationToken ct)
    {
        if (_agg.Count == 0) return;

        // snapshot e troca de referência (constante no tempo)
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
                PercentualSucesso = m.TotalRequests > 0
                    ? (decimal)m.SuccessfulRequests / m.TotalRequests
                    : 0
            });
        }

        // persiste em escopo isolado (retry simples para lidar com SQLite locked)
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITelemetriaRepository>();

        const int maxRetries = 3;
        var delay = TimeSpan.FromMilliseconds(150);

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await repository.SalvarTelemetriaAsync(registros);
                break;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex,
                    "Falha ao salvar telemetria (tentativa {Attempt}/{Max}). Aguardando {Delay}…",
                    attempt, maxRetries, delay);
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // backoff exponencial
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar telemetria após {Max} tentativas.", maxRetries);
                break; // não propaga (telemetria não deve quebrar app)
            }
        }
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
