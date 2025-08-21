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