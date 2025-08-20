using hackathon.Application.Interfaces;
using hackathon.Application.Dtos;
using hackathon.Domain.Entities;

namespace hackathon.Tests.UseCases;

public class ObterSimulacoesUseCaseTests
{
    private class SimulacaoRepositoryFake : ISimulacaoRepository
    {
        public Task SalvarAsync(Simulacao simulacao)
            => throw new NotImplementedException();

        public Task<PaginacaoResultado<SimulacaoResumo>> ObterPaginadoAsync(int pagina, int qtdRegistrosPagina)
        {
            var registros = new List<SimulacaoResumo>
            {
                new() { IdSimulacao = 1, ValorDesejado = 10000, Prazo = 12, ValorTotalParcelas = 10500 },
                new() { IdSimulacao = 2, ValorDesejado = 5000, Prazo = 6, ValorTotalParcelas = 5300 }
            };

            var resultado = new PaginacaoResultado<SimulacaoResumo>
            {
                Pagina = pagina,
                QtdRegistros = registros.Count,
                QtdRegistrosPagina = qtdRegistrosPagina,
                Registros = registros
            };

            return Task.FromResult(resultado);
        }
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarSimulacoesPaginadas()
    {
        // Arrange
        var repository = new SimulacaoRepositoryFake();
        var useCase = new ObterSimulacoesUseCase(repository);
        var request = new ListarRequest { Pagina = 1, QtdRegistrosPagina = 10 };

        // Act
        var resultado = await useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(2, resultado.QtdRegistros);
        Assert.Equal(10, resultado.QtdRegistrosPagina);
        Assert.Equal(2, resultado.Registros.Count);
        Assert.Contains(resultado.Registros, r => r.IdSimulacao == 1);
        Assert.Contains(resultado.Registros, r => r.IdSimulacao == 2);
    }
}
