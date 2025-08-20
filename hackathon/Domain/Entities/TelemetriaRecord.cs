namespace hackathon.Domain.Entities;

public class TelemetriaRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime DataReferencia { get; set; }
    public string NomeApi { get; set; } = string.Empty;
    public int QtdRequisicoes { get; set; }
    public int TempoMedio { get; set; }
    public int TempoMinimo { get; set; }
    public int TempoMaximo { get; set; }
    public decimal PercentualSucesso { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
