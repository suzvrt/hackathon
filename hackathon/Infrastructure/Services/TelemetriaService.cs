using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Application.Dtos;
using hackathon.Infrastructure.Persistence;
using System.Collections.Concurrent;
using System.Globalization;

namespace hackathon.Infrastructure.Services;

public class TelemetriaService : ITelemetriaService, IDisposable
{
    private readonly ITelemetriaRepository _repository;
    private readonly ConcurrentDictionary<string, MetricasEndpoint> _endpointMetrics;
    private readonly Timer _flushTimer;
    private readonly object _lockObject = new();
    private bool _disposed = false;

    public TelemetriaService(ITelemetriaRepository repository)
    {
        _repository = repository;
        _endpointMetrics = new ConcurrentDictionary<string, MetricasEndpoint>();
        
        // Descarregar dados de telemetria a cada 5 minutos para minimizar uso de memória
        _flushTimer = new Timer(FlushTelemetriaData, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public void RegistrarRequisicao(string nomeEndpoint, TimeSpan duracao, bool sucesso)
    {
        if (_disposed) return;

        var metricas = _endpointMetrics.GetOrAdd(nomeEndpoint, _ => new MetricasEndpoint());
        
        lock (metricas)
        {
            metricas.TotalRequests++;
            metricas.SuccessfulRequests += sucesso ? 1 : 0;
            
            var durationMs = (int)duracao.TotalMilliseconds;
            metricas.TotalDuration += durationMs;
            
            if (metricas.MinDuration == 0 || durationMs < metricas.MinDuration)
                metricas.MinDuration = durationMs;
                
            if (metricas.MaxDuration < durationMs)
                metricas.MaxDuration = durationMs;
        }
    }

    public async Task<TelemetriaResponse> ObterTelemetriaAsync(DateTime dataReferencia)
    {
        // Primeiro descarregar dados pendentes
        await FlushTelemetriaDataAsync();
        
        var registros = await _repository.ObterTelemetriaPorDataAsync(dataReferencia);
        
        var dataFormatada = dataReferencia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

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

    private void FlushTelemetriaData(object? state)
    {
        _ = FlushTelemetriaDataAsync();
    }

    private async Task FlushTelemetriaDataAsync()
    {
        if (_disposed || _endpointMetrics.IsEmpty) return;

        var registrosParaSalvar = new List<TelemetriaRecord>();
        var hoje = DateTime.Today;

        lock (_lockObject)
        {
            foreach (var kvp in _endpointMetrics)
            {
                var metricas = kvp.Value;
                var record = new TelemetriaRecord
                {
                    DataReferencia = hoje,
                    NomeApi = kvp.Key,
                    QtdRequisicoes = metricas.TotalRequests,
                    TempoMedio = metricas.TotalRequests > 0 ? (int)(metricas.TotalDuration / metricas.TotalRequests) : 0,
                    TempoMinimo = metricas.MinDuration,
                    TempoMaximo = metricas.MaxDuration,
                    PercentualSucesso = metricas.TotalRequests > 0 
                        ? (decimal)metricas.SuccessfulRequests / metricas.TotalRequests 
                        : 0
                };

                registrosParaSalvar.Add(record);
            }

            // Limpar as métricas após salvar
            _endpointMetrics.Clear();
        }

        if (registrosParaSalvar.Any())
        {
            try
            {
                await _repository.SalvarTelemetriaAsync(registrosParaSalvar);
            }
            catch (Exception ex)
            {
                // Registrar erro mas não lançar exceção - telemetria não deve quebrar a aplicação principal
                Console.WriteLine($"Erro ao descarregar dados de telemetria: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _flushTimer?.Dispose();
            
            // Descarregamento final antes da disposição
            _ = FlushTelemetriaDataAsync().Wait(TimeSpan.FromSeconds(5));
        }
    }

    private class MetricasEndpoint
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public long TotalDuration { get; set; }
        public int MinDuration { get; set; }
        public int MaxDuration { get; set; }
    }
}
