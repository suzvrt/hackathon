namespace hackathon.Infrastructure.Config;

public class EventHubSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public string EventHubName { get; init; } = string.Empty;
}