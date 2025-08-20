using hackathon.Application.Interfaces;

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

        try
        {
            // Processar a requisição
            await _next(context);
        }
        finally
        {
            cronometro.Stop();

            // Registrar dados de telemetria de forma assíncrona para evitar bloqueio
            _ = Task.Run(() =>
            {
                try
                {
                    var nomeEndpoint = GetEndpointName(context);
                    var duracao = cronometro.Elapsed;
                    var sucesso = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300;

                    _telemetriaService.RegistrarRequisicao(nomeEndpoint, duracao, sucesso);
                }
                catch
                {
                    // Falhar silenciosamente - telemetria não deve quebrar a aplicação principal
                }
            });
        }
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
