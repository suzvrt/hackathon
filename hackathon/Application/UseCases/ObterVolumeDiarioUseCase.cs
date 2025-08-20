using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;

public sealed class ObterVolumeDiarioUseCase : IObterVolumeDiarioUseCase
{
    private readonly IProdutoRepository _repository;

    public ObterVolumeDiarioUseCase(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<VolumeSimuladoDiario> ExecutarAsync(DateOnly dataReferencia)
    {
        return await _repository.ObterVolumeSimuladoPorDiaAsync(dataReferencia);
    }
}
