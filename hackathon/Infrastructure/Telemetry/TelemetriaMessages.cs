namespace hackathon.Infrastructure.Telemetry;

public abstract record TelemetriaMessage;

public sealed record TelemetriaEvent(
    string Endpoint,
    int DurationMs,
    bool Success) : TelemetriaMessage;

public sealed record TelemetriaFlush(
    TaskCompletionSource<bool> Tcs) : TelemetriaMessage;