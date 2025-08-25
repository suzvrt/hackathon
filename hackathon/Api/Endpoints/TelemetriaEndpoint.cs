using Microsoft.AspNetCore.Mvc;
using hackathon.Infrastructure.Telemetria;
using hackathon.Application.Interfaces;

namespace hackathon.Api.Endpoints;

public static class TelemetriaEndpoint
{
    public static void MapTelemetria(this WebApplication app)
    {
        app.MapGet("/telemetria", async (
            [FromQuery] DateOnly? dataReferencia,
            ITelemetriaService telemetriaService) =>
        {
            var data = dataReferencia ?? DateOnly.FromDateTime(DateTime.Today);
            var resultado = await telemetriaService.ObterTelemetriaAsync(data);
            return Results.Ok(resultado);
        })
        .WithDisplayName("ObterTelemetria")
        .WithMetadata(new ExcluirTelemetriaAttribute());
    }
}