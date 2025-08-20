using System.Text.Json.Serialization;

namespace hackathon.Application.Dtos;

public sealed class SimulacaoResumo
{
    [JsonPropertyName("idSimulacao")]
    public int IdSimulacao { get; init; }

    [JsonPropertyName("valorDesejado")]
    public decimal ValorDesejado { get; init; }

    [JsonPropertyName("prazo")]
    public int Prazo { get; init; }

    [JsonPropertyName("valorTotalParcelas")]
    public decimal ValorTotalParcelas { get; init; }
}

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
