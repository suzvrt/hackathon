using hackathon.Infrastructure.Services;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;

namespace hackathon.Tests.Services;

public class TelemetriaServiceTests
{
    private class TelemetriaRepositoryFake : ITelemetriaRepository
    {
        public List<TelemetriaRecord> Persistidos { get; } = new();

        public Task SalvarTelemetriaAsync(List<TelemetriaRecord> records)
        {
            Persistidos.AddRange(records);
            return Task.CompletedTask;
        }

        public Task<List<TelemetriaRecord>> ObterTelemetriaPorDataAsync(DateTime dataReferencia)
        {
            return Task.FromResult(Persistidos.FindAll(r => r.DataReferencia.Date == dataReferencia.Date));
        }
    }

    [Fact]
    public async Task RegistrarRequisicao_DeveAcumularMetricasCorretamente()
    {
        // Arrange
        var repo = new TelemetriaRepositoryFake();
        var service = new TelemetriaService(repo);

        // Act
        service.RegistrarRequisicao("GET simulacoes", TimeSpan.FromMilliseconds(100), true);
        service.RegistrarRequisicao("GET simulacoes", TimeSpan.FromMilliseconds(200), false);
        await service.ObterTelemetriaAsync(DateTime.Today); // força flush

        // Assert
        Assert.Single(repo.Persistidos);
        var record = repo.Persistidos[0];
        Assert.Equal("GET simulacoes", record.NomeApi);
        Assert.Equal(2, record.QtdRequisicoes);
        Assert.Equal(150, record.TempoMedio);
        Assert.Equal(100, record.TempoMinimo);
        Assert.Equal(200, record.TempoMaximo);
        Assert.Equal(0.5m, record.PercentualSucesso);
    }

    [Fact]
    public async Task ObterTelemetriaAsync_DeveRetornarTelemetriaResponseComDados()
    {
        // Arrange
        var repo = new TelemetriaRepositoryFake();
        var hoje = DateTime.Today;
        repo.Persistidos.Add(new TelemetriaRecord
        {
            DataReferencia = hoje,
            NomeApi = "POST simulacoes",
            QtdRequisicoes = 10,
            TempoMedio = 120,
            TempoMinimo = 100,
            TempoMaximo = 150,
            PercentualSucesso = 0.9m
        });

        var service = new TelemetriaService(repo);

        // Act
        var response = await service.ObterTelemetriaAsync(hoje);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(hoje.ToString("yyyy-MM-dd"), response.DataReferencia);
        Assert.Single(response.ListaEndpoints);
        var endpoint = response.ListaEndpoints[0];
        Assert.Equal("POST simulacoes", endpoint.NomeApi);
        Assert.Equal(10, endpoint.QtdRequisicoes);
        Assert.Equal(120, endpoint.TempoMedio);
        Assert.Equal(100, endpoint.TempoMinimo);
        Assert.Equal(150, endpoint.TempoMaximo);
        Assert.Equal(0.9m, endpoint.PercentualSucesso);
    }
}
