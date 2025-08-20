using System.Text.Json.Serialization;
using hackathon.Domain.ValueObjects;

namespace hackathon.Application.Dtos;

public record ParcelaSimulada(
    [property: JsonPropertyName("numero")] int Numero,
    [property: JsonPropertyName("valorAmortizacao")] decimal ValorAmortizacao,
    [property: JsonPropertyName("valorJuros")] decimal ValorJuros,
    [property: JsonPropertyName("valorPrestacao")] decimal ValorPrestacao);

public record ResultadoSimulacao(
    [property: JsonPropertyName("tipo")] string Tipo,
    [property: JsonPropertyName("parcelas")] List<ParcelaSimulada> Parcelas
);

public record SimulacaoResponse(
    [property: JsonPropertyName("idSimulacao")] int IdSimulacao,
    [property: JsonPropertyName("codigoProduto")] int CodigoProduto,
    [property: JsonPropertyName("descricaoProduto")] string DescricaoProduto,
    [property: JsonPropertyName("taxaJuros")] decimal TaxaJuros,
    [property: JsonPropertyName("sac")] ResultadoSimulacao Sac,
    [property: JsonPropertyName("price")] ResultadoSimulacao Price
);

public static class ParcelaMapper
{
    public static ParcelaSimulada ToDto(this Parcela p) =>
        new(p.Numero, decimal.Round(p.ValorAmortizacao, 2), decimal.Round(p.ValorJuros, 2), decimal.Round(p.ValorPrestacao, 2));
}
