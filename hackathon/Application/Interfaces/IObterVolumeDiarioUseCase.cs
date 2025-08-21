using hackathon.Application.Dtos;

namespace hackathon.Application.Interfaces;

public interface IObterVolumeDiarioUseCase
{
    Task<VolumeSimuladoDiario> ExecutarAsync(DateOnly dataReferencia, string? sistema);
}