using hackathon.Domain.Entities;

namespace hackathon.Application.Interfaces;

public interface ISimulacaoPersistenceService
{
    Task EnqueueAsync(Simulacao simulacao);
}
