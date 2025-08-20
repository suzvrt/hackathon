using System.Text.Json.Serialization;

namespace hackathon.Application.Dtos;

public record SimulacaoRequest(
    [property: JsonPropertyName("valorDesejado")] decimal ValorDesejado,
    [property: JsonPropertyName("prazo")] int Prazo);