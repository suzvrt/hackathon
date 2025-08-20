using hackathon.Application.Interfaces;
using hackathon.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace hackathon.Api.Endpoints;

public static class TelemetriaEndpoint
{
    public static void MapTelemetria(this WebApplication app)
    {
        app.MapGet("/Telemetria", async (
            [FromQuery] DateTime dataReferencia,
            ITelemetriaService telemetriaService) =>
        {
            var data = dataReferencia == default ? DateTime.Today : dataReferencia;
            var resultado = await telemetriaService.ObterTelemetriaAsync(data);
            return Results.Ok(resultado);
        }).WithDisplayName("ObterTelemetria");
    }
}
