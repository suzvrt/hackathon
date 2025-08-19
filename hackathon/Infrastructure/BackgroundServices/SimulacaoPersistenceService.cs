using System.Threading.Channels;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;

namespace hackathon.Infrastructure.BackgroundServices;

public class SimulacaoPersistenceService : BackgroundService, ISimulacaoPersistenceService
{
    private readonly Channel<Simulacao> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SimulacaoPersistenceService> _logger;

    public SimulacaoPersistenceService(
        IServiceScopeFactory scopeFactory,
        ILogger<SimulacaoPersistenceService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _channel = Channel.CreateUnbounded<Simulacao>();
    }

    public async Task EnqueueAsync(Simulacao simulacao)
    {
        await _channel.Writer.WriteAsync(simulacao);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var simulacao = await _channel.Reader.ReadAsync(stoppingToken);
                
                using var scope = _scopeFactory.CreateScope();
                var simulacaoRepository = scope.ServiceProvider.GetRequiredService<ISimulacaoRepository>();
                var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                await simulacaoRepository.SalvarAsync(simulacao);
                await eventPublisher.PublishAsync(simulacao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing simulation persistence");
            }
        }
    }
}