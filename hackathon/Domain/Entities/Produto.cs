namespace hackathon.Domain.Entities;

public class Produto
{
    public int Codigo { get; init; }
    public string Nome { get; init; } = string.Empty;
    public decimal TaxaJuros { get; init; }
    public short MinimoMeses { get; init; }
    public short? MaximoMeses { get; init; }
    public decimal ValorMinimo { get; init; }
    public decimal? ValorMaximo { get; init; }

    public bool EhCompativel(decimal valor, int prazo) =>
        valor >= ValorMinimo &&
        (ValorMaximo == null || valor <= ValorMaximo) &&
        prazo >= MinimoMeses &&
        (MaximoMeses == null || prazo <= MaximoMeses);
}
