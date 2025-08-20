using hackathon.Domain.Entities;
using hackathon.Application.Dtos;

namespace hackathon.Application.Interfaces;

public interface ITelemetriaService
{
    void RegistrarRequisicao(string nomeEndpoint, TimeSpan duracao, bool sucesso);
    Task<TelemetriaResponse> ObterTelemetriaAsync(DateTime dataReferencia);
}
