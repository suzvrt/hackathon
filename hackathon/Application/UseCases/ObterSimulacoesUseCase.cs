using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;

public sealed class ObterSimulacoesUseCase : IObterSimulacoesUseCase
{
    private readonly ISimulacaoRepository _repository;

    public ObterSimulacoesUseCase(ISimulacaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginacaoResultado<SimulacaoResumo>> ExecutarAsync(ListarRequest request)
    {
        return await _repository.ObterPaginadoAsync(request.Pagina, request.QtdRegistrosPagina, request.Sistema);
    }
}
