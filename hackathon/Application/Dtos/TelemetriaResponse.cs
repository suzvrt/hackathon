namespace hackathon.Application.Dtos;

public class TelemetriaResponse
{
    public string DataReferencia { get; set; } = "";
    public List<EndpointTelemetria> ListaEndpoints { get; set; } = new();
}

public class EndpointTelemetria
{
    public string NomeApi { get; set; } = string.Empty;
    public int QtdRequisicoes { get; set; }
    public int TempoMedio { get; set; }
    public int TempoMinimo { get; set; }
    public int TempoMaximo { get; set; }
    public decimal PercentualSucesso { get; set; }
}
