using hackathon.Application.Dtos;
using hackathon.Domain.ValueObjects;

namespace hackathon.Domain.Entities;

public class Simulacao
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int CodigoProduto { get; init; }
    public string DescricaoProduto { get; init; } = string.Empty;
    public decimal TaxaJuros { get; init; }
    public DateTime CriadoEm { get; init; } = DateTime.UtcNow;

    public List<Parcela> Sac { get; init; } = [];
    public List<Parcela> Price { get; init; } = [];
}
