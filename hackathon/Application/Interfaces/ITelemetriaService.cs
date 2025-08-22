using hackathon.Domain.Entities;
using hackathon.Application.Dtos;

namespace hackathon.Application.Interfaces;

public interface ITelemetriaService
{
    Task<TelemetriaResponse> ObterTelemetriaAsync(DateOnly dataReferencia);
    void RegistrarRequisicao(string nomeEndpoint, TimeSpan duracao, bool sucesso);
    Task RegistrarRequisicaoAsync(string nomeEndpoint, TimeSpan duracao, bool sucesso);
}
