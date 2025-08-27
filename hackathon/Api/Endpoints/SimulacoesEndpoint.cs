using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;

namespace hackathon.Api.Endpoints;

public static class SimulacoesEndpoint
{
    public static void MapSimulacoes(this WebApplication app)
    {
        app.MapPost("/simulacoes", async (SimulacaoRequest request, ISimularEmprestimoUseCase useCase) =>
        {
            var resultado = await useCase.ExecutarAsync(request);
            return resultado is null ? Results.UnprocessableEntity("Nenhum produto compatível encontrado.") : Results.Ok(resultado);
        }).WithDisplayName("SimularEmprestimo");

        app.MapGet("/simulacoes", async ([AsParameters] ListarRequest request, IObterSimulacoesUseCase useCase) =>
        {
            var resultado = await useCase.ExecutarAsync(request);
            return Results.Ok(resultado);
        }).WithDisplayName("ObterSimulacoes");
    }
}
