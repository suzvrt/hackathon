using hackathon.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace hackathon.Api.Endpoints;

public static class ProdutosEndpoint
{
    public static void MapProdutos(this WebApplication app)
    {
        app.MapGet("/produtos", async (
            [FromQuery] DateOnly dataReferencia,
            IObterVolumeDiarioUseCase useCase,
            [FromQuery] string? sistema = "PRICE"
            ) =>
        {
            var resultado = await useCase.ExecutarAsync(dataReferencia, sistema);
            return Results.Ok(resultado);
        }).WithDisplayName("ObterVolumeDiario");
    }
}
