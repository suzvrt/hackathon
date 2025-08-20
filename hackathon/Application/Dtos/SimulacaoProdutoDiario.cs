using System.Text.Json.Serialization;

namespace hackathon.Application.Dtos;

public class SimulacaoProdutoDiario
{
    [JsonPropertyName("codigoProduto")]
    public int CodigoProduto { get; set; }

    [JsonPropertyName("descricaoProduto")]
    public string DescricaoProduto { get; set; } = "";

    [JsonPropertyName("taxaMediaJuro")]
    public decimal TaxaMediaJuro { get; set; }

    [JsonPropertyName("valorMedioPrestacao")]
    public decimal ValorMedioPrestacao { get; set; }

    [JsonPropertyName("valorTotalDesejado")]
    public decimal ValorTotalDesejado { get; set; }

    [JsonPropertyName("valorTotalCredito")]
    public decimal ValorTotalCredito { get; set; }
}