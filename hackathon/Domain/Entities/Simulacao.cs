using hackathon.Application.Dto;

namespace hackathon.Domain.Entities;

public class Simulacao
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int CodigoProduto { get; init; }
    public string DescricaoProduto { get; init; } = string.Empty;
    public decimal TaxaJuros { get; init; }
    public DateTime CriadoEm { get; init; } = DateTime.UtcNow;

    public ResultadoSimulacao Sac { get; init; } = new("SAC", []);
    public ResultadoSimulacao Price { get; init; } = new("PRICE", []);
}
