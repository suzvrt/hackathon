using hackathon.Domain.ValueObjects;

namespace hackathon.Application.Dtos;

public record ParcelaSimulada(int Numero, decimal ValorAmortizacao, decimal ValorJuros, decimal ValorPrestacao);

public record ResultadoSimulacao(
    string Tipo,
    List<ParcelaSimulada> Parcelas
);

public record SimulacaoResponse(
    int IdSimulacao,
    int CodigoProduto,
    string DescricaoProduto,
    decimal TaxaJuros,
    ResultadoSimulacao Sac,
    ResultadoSimulacao Price
);

public static class ParcelaMapper
{
    public static ParcelaSimulada ToDto(this Parcela p) =>
        new(p.Numero, decimal.Round(p.ValorAmortizacao, 2), decimal.Round(p.ValorJuros, 2), decimal.Round(p.ValorPrestacao, 2));
}
