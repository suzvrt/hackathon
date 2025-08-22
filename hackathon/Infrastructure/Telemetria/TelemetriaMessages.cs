using System;
using System.Threading.Tasks;

namespace hackathon.Infrastructure.Telemetria;

public abstract record TelemetriaMessage;

public readonly record struct TelemetriaEvent
{
    public string EndpointName { get; init; }
    public double DurationMs { get; init; } // CORREÇÃO: Agora armazenamos milissegundos diretamente
    public int StatusCode { get; init; }
    public DateTime Timestamp { get; init; }
}

public sealed record TelemetriaEventMessage(TelemetriaEvent Payload) : TelemetriaMessage;

public sealed record TelemetriaFlush(
    TaskCompletionSource<bool> Tcs) : TelemetriaMessage;
