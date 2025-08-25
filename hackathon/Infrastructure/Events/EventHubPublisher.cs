using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using hackathon.Api.Serialization;
using hackathon.Application.Interfaces;
using hackathon.Infrastructure.Config;
using System.Text.Json;

namespace hackathon.Infrastructure.Events;

public class EventHubPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly EventHubProducerClient _producerClient;
    private readonly ILogger<EventHubPublisher> _logger;

    public EventHubPublisher(EventHubSettings settings,
        ILogger<EventHubPublisher> logger)
    {
        _producerClient = new EventHubProducerClient(settings.ConnectionString);
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventData = new EventData(
                JsonSerializer.SerializeToUtf8Bytes(message, AppJsonSerializerContext.Default.String)
            );
            await _producerClient.SendAsync([eventData], cancellationToken);
            _logger.LogInformation($"Evento publicado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Falha ao publicar evento: {ex.Message}");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _producerClient.DisposeAsync();
    }
}