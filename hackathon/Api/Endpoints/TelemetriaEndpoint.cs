using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hackathon.Infrastructure.Telemetria;
using hackathon.Infrastructure.Services;
using hackathon.Application.Interfaces;

namespace hackathon.Api.Endpoints;

public static class TelemetriaEndpoint
{
    public static void MapTelemetria(this WebApplication app)
    {
        app.MapGet("/telemetria", async (
            [FromQuery] DateTime dataReferencia,
            ITelemetriaService telemetriaService) =>
        {
            var data = dataReferencia == default ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.FromDateTime(dataReferencia);
            var resultado = await telemetriaService.ObterTelemetriaAsync(data);
            return Results.Ok(resultado);
        }).WithDisplayName("ObterTelemetria")
        .WithMetadata(new ExcluirTelemetriaAttribute());
    }
}
