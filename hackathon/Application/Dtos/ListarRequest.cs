using System.Text.Json.Serialization;

namespace hackathon.Application.Dtos;

public record ListarRequest(
    [property: JsonPropertyName("pagina")] int Pagina = 1,
    [property: JsonPropertyName("qtdRegistrosPagina")] int QtdRegistrosPagina = 200,
    [property: JsonPropertyName("sistema")] string Sistema = "PRICE");