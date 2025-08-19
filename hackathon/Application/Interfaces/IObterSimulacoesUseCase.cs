using hackathon.Application.Dtos;

namespace hackathon.Application.Interfaces;

public interface IObterSimulacoesUseCase
{
    Task<PaginacaoResultado<SimulacaoResumo>> ExecutarAsync(ListarRequest request);
}
