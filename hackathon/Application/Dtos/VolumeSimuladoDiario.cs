using System.Text.Json.Serialization;

namespace hackathon.Application.Dtos;

public class VolumeSimuladoDiario
{
    [JsonPropertyName("dataReferencia")]
    public string DataReferencia { get; set; } = "";

    [JsonPropertyName("simulacoes")]
    public List<SimulacaoProdutoDiario> Simulacoes { get; set; } = [];
}