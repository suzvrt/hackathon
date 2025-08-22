using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using hackathon.Application.Interfaces;
using hackathon.Infrastructure.Telemetria;

namespace hackathon.Api.Middleware;

public class TelemetriaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITelemetriaService _telemetriaService;

    public TelemetriaMiddleware(RequestDelegate next, ITelemetriaService telemetriaService)
    {
        _next = next;
        _telemetriaService = telemetriaService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cronometro = System.Diagnostics.Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        bool shouldExcludeFromTelemetry = false;

        try
        {
            // Verificar se o endpoint deve ser excluído da telemetria antes de processar
            shouldExcludeFromTelemetry = ShouldExcludeFromTelemetry(context);
            
            // Processar a requisição
            await _next(context);
        }
        finally
        {
            cronometro.Stop();
        }

        // Lógica de telemetria movida para fora do finally para permitir return
        if (shouldExcludeFromTelemetry)
        {
            return;
        }

        try
        {
            var nomeEndpoint = GetEndpointName(context);
            var duracao = cronometro.Elapsed;
            var sucesso = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300;

            await _telemetriaService.RegistrarRequisicaoAsync(nomeEndpoint, duracao, sucesso);
        }
        catch
        {
            // Falhar silenciosamente - telemetria não deve quebrar a aplicação principal
        }
    }

    private static bool ShouldExcludeFromTelemetry(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null) return false;

        // Verifica se o endpoint tem o atributo ExcluirTelemetriaAttribute
        var excludeAttribute = endpoint.Metadata.GetMetadata<ExcluirTelemetriaAttribute>();
        return excludeAttribute != null;
    }

    private static string GetEndpointName(HttpContext context)
    {
        var method = context.Request.Method;
        var endpoint = context.GetEndpoint();

        var routePattern = endpoint is RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText
            : context.Request.Path.Value;

        // Normaliza rota
        var route = string.IsNullOrEmpty(routePattern) ? "/" : routePattern.StartsWith("/") ? routePattern : "/" + routePattern;

        return $"{method} {route}";
    }
}

public static class TelemetriaMiddlewareExtensions
{
    public static IApplicationBuilder UseTelemetria(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TelemetriaMiddleware>();
    }
}
