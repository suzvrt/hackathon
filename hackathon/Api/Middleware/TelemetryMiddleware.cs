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
        var endpoint = context.GetEndpoint();
        if (endpoint?.DisplayName != null)
        {
            // Extrair nome significativo do endpoint
            var displayName = endpoint.DisplayName;
            if (displayName.Contains("ObterSimulacoes"))
                return "ObterSimulacoes";
            if (displayName.Contains("SimularEmprestimo"))
                return "SimularEmprestimo";
            if (displayName.Contains("ObterVolumeDiario"))
                return "ObterVolumeDiario";
            if (displayName.Contains("Telemetria"))
                return "ObterTelemetria";
        }

        // Fallback para padrão de rota
        var route = context.Request.Path.Value?.TrimStart('/') ?? "Desconhecido";
        return string.IsNullOrEmpty(route) ? "Raiz" : route;
    }
}

public static class TelemetriaMiddlewareExtensions
{
    public static IApplicationBuilder UseTelemetria(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TelemetriaMiddleware>();
    }
}
