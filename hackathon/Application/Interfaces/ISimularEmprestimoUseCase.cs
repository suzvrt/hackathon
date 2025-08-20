using hackathon.Application.Dtos;

namespace hackathon.Application.Interfaces;

public interface ISimularEmprestimoUseCase
{
    Task<SimulacaoResponse?> ExecutarAsync(SimulacaoRequest request);
}