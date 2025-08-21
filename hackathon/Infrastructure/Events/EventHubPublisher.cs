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

    public EventHubPublisher(EventHubSettings settings)
    {
        _producerClient = new EventHubProducerClient(settings.ConnectionString);
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventData = new EventData(
                JsonSerializer.SerializeToUtf8Bytes(message, AppJsonSerializerContext.Default.String)
            );
            await _producerClient.SendAsync([eventData], cancellationToken);
            Console.WriteLine($"Evento publicado com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Falha ao publicar evento: {ex.Message}");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _producerClient.DisposeAsync();
    }
}