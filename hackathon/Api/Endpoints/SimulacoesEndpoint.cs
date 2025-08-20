using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;
using hackathon.Application.UseCases;

namespace hackathon.Api.Endpoints;

public static class SimulacoesEndpoint
{
    public static void MapSimulacoes(this WebApplication app)
    {
        app.MapPost("/simulacoes", async (SimulacaoRequest request, ISimularEmprestimoUseCase useCase) =>
        {
            var resultado = await useCase.ExecutarAsync(request);
            return resultado is null ? Results.NotFound("Nenhum produto compatível encontrado.") : Results.Ok(resultado);
        });

        app.MapGet("/simulacoes", async ([AsParameters] ListarRequest request, IObterSimulacoesUseCase useCase) =>
        {
            var resultado = await useCase.ExecutarAsync(request);
            return Results.Ok(resultado);
        });
    }
}
