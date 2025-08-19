using System.Threading.Channels;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;

namespace hackathon.Infrastructure.BackgroundServices;

public class SimulacaoPersistenceService : BackgroundService, ISimulacaoPersistenceService
{
    private readonly Channel<Simulacao> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SimulacaoPersistenceService> _logger;
    private readonly object _lock = new();
    private bool _isProcessing;

    public SimulacaoPersistenceService(
        IServiceScopeFactory scopeFactory,
        ILogger<SimulacaoPersistenceService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _channel = Channel.CreateUnbounded<Simulacao>(new UnboundedChannelOptions 
        { 
            SingleReader = true,
            SingleWriter = false 
        });
    }

    public async Task EnqueueAsync(Simulacao simulacao)
    {
        try
        {
            await _channel.Writer.WriteAsync(simulacao);
            
            bool shouldStartProcessing = false;
            lock (_lock)
            {
                if (!_isProcessing)
                {
                    _isProcessing = true;
                    shouldStartProcessing = true;
                }
            }

            if (shouldStartProcessing)
            {
                _ = ProcessChannelAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enfileirar simulação");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await ProcessChannelAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Erro no processamento do background service");
        }
    }

    private async Task ProcessChannelAsync(CancellationToken stoppingToken = default)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await _channel.Reader.WaitToReadAsync(stoppingToken))
                {
                    while (_channel.Reader.TryRead(out var simulacao))
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var simulacaoRepository = scope.ServiceProvider.GetRequiredService<ISimulacaoRepository>();
                            await simulacaoRepository.SalvarAsync(simulacao);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao processar simulação {SimulacaoId}", simulacao.Id);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento do channel");
                await Task.Delay(1000, stoppingToken);
            }
        }

        lock (_lock)
        {
            _isProcessing = false;
        }
    }
}