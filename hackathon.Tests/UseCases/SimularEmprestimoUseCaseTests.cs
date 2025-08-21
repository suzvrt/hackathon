using hackathon.Application.UseCases;
using hackathon.Application.Dtos;
using hackathon.Domain.Entities;
using hackathon.Application.Interfaces;

namespace hackathon.Tests.UseCases;

public class SimularEmprestimoUseCaseTests
{
    private class ProdutoRepositoryFake : IProdutoRepository
    {
        public Task<IEnumerable<Produto>> ObterProdutosCompativeisAsync(decimal valor, int prazo)
        {
            var produtos = new List<Produto>
        {
            new Produto
            {
                Codigo = 1,
                Nome = "Produto A",
                TaxaJuros = 0.05m,
                ValorMinimo = 1000,
                ValorMaximo = 20000,
                MinimoMeses = 6,
                MaximoMeses = 24
            },
            new Produto
            {
                Codigo = 2,
                Nome = "Produto B",
                TaxaJuros = 0.03m,
                ValorMinimo = 500,
                ValorMaximo = 15000,
                MinimoMeses = 6,
                MaximoMeses = 24
            }
        };

            return Task.FromResult(produtos.Where(p => p.EhCompativel(valor, prazo)));
        }

        public Task<VolumeSimuladoDiario> ObterVolumeSimuladoPorDiaAsync(DateOnly dataReferencia, string? sistema)
            => throw new NotImplementedException();

    }

    private class SimulacaoPersistenceFake : ISimulacaoPersistenceService
    {
        public List<Simulacao> Persistidas { get; } = new();

        public Task EnqueueAsync(Simulacao simulacao)
        {
            Persistidas.Add(simulacao);
            return Task.CompletedTask;
        }
    }

    private class EventPublisherFake : IEventPublisher
    {
        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarSimulacaoResponse_QuandoProdutoCompativelExiste()
    {
        // Arrange
        var repo = new ProdutoRepositoryFake();
        var persistence = new SimulacaoPersistenceFake();
        var eventPublisher = new EventPublisherFake();
        var useCase = new SimularEmprestimoUseCase(repo, persistence, eventPublisher);

        var request = new SimulacaoRequest(10000, 12);

        // Act
        var response = await useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.CodigoProduto); // Produto B tem menor taxa
        Assert.Equal("Produto B", response.DescricaoProduto);
        Assert.Equal(0.03m, response.TaxaJuros);
        Assert.Equal(12, response.Sac.Parcelas.Count);
        Assert.Equal(12, response.Price.Parcelas.Count);
        Assert.Single(persistence.Persistidas);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarNull_QuandoNenhumProdutoCompativel()
    {
        // Arrange
        var repo = new ProdutoRepositoryFake();
        var persistence = new SimulacaoPersistenceFake();
        var eventPublisher = new EventPublisherFake();
        var useCase = new SimularEmprestimoUseCase(repo, persistence, eventPublisher);

        var request = new SimulacaoRequest(999999, 99); // fora dos limites

        // Act
        var response = await useCase.ExecutarAsync(request);

        // Assert
        Assert.Null(response);
        Assert.Empty(persistence.Persistidas);
    }
}
