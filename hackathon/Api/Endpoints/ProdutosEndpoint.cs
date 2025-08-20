using hackathon.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace hackathon.Api.Endpoints;

public static class ProdutosEndpoint
{
    public static void MapProdutos(this WebApplication app)
    {
        app.MapGet("/Volume", async (
            [FromQuery] DateOnly dataReferencia, // O RDG sabe como ler isso da querystring
            IObterVolumeDiarioUseCase useCase   // O RDG pede ao DI para injetar o use case
        ) =>
        {
            // O corpo do endpoint é uma chamada direta ao use case.
            // O resultado (um DTO) será serializado para JSON automaticamente.
            var resultado = await useCase.ExecutarAsync(dataReferencia);
            return Results.Ok(resultado);

        });
    }
}
