using System.Text.Json.Serialization;

namespace hackathon.Application.Dtos;

public sealed class PaginacaoResultado<T>
{
    [JsonPropertyName("pagina")]
    public int Pagina { get; init; }

    [JsonPropertyName("qtdRegistros")]
    public int QtdRegistros { get; init; }

    [JsonPropertyName("qtdRegistrosPagina")]
    public int QtdRegistrosPagina { get; init; }

    [JsonPropertyName("registros")]
    public IReadOnlyList<T> Registros { get; init; } = [];
}