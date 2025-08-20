using hackathon.Domain.Entities;

namespace hackathon.Application.Interfaces;

public interface ITelemetriaRepository
{
    Task SalvarTelemetriaAsync(List<TelemetriaRecord> records);
    Task<List<TelemetriaRecord>> ObterTelemetriaPorDataAsync(DateTime dataReferencia);
}