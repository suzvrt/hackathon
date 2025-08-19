using hackathon.Domain.Entities;

namespace hackathon.Application.Interfaces;

public interface ISimulacaoRepository
{
    Task SalvarAsync(Simulacao simulacao);
}