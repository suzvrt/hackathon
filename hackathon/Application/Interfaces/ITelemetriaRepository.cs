using hackathon.Domain.Entities;

namespace hackathon.Application.Interfaces;

public interface ITelemetriaRepository
{
    Task SalvarTelemetriaAsync(List<TelemetriaRecord> records);
    Task<IEnumerable<TelemetriaRecord>> ObterTelemetriaPorDataAsync(DateOnly dataReferencia);
}
