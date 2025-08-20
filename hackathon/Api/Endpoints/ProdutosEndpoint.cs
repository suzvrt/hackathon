using hackathon.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace hackathon.Api.Endpoints;

public static class ProdutosEndpoint
{
    public static void MapProdutos(this WebApplication app)
    {
        app.MapGet("/Volume", async ([FromQuery] DateOnly dataReferencia, IObterVolumeDiarioUseCase useCase) =>
        {
            var resultado = await useCase.ExecutarAsync(dataReferencia);
            return Results.Ok(resultado);
        }).WithName("ObterVolumeDiario");
    }
}
