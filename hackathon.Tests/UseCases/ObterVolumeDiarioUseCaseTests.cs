using hackathon.Application.Interfaces;
using hackathon.Application.Dtos;
using hackathon.Domain.Entities;

namespace hackathon.Tests.UseCases;

public class ObterVolumeDiarioUseCaseTests
{
    private class ProdutoRepositoryFake : IProdutoRepository
    {
        public Task<IEnumerable<Produto>> ObterProdutosCompativeisAsync(decimal valor, int prazo)
            => throw new NotImplementedException();

        public Task<VolumeSimuladoDiario> ObterVolumeSimuladoPorDiaAsync(DateOnly dataReferencia)
        {
            var resultado = new VolumeSimuladoDiario
            {
                DataReferencia = dataReferencia.ToString("yyyy-MM-dd"),
                Simulacoes = new List<SimulacaoProdutoDiario>
                {
                    new()
                    {
                        CodigoProduto = 1,
                        DescricaoProduto = "Produto A",
                        TaxaMediaJuro = 0.035m,
                        ValorMedioPrestacao = 850.75m,
                        ValorTotalDesejado = 10000m,
                        ValorTotalCredito = 10209m
                    },
                    new()
                    {
                        CodigoProduto = 2,
                        DescricaoProduto = "Produto B",
                        TaxaMediaJuro = 0.045m,
                        ValorMedioPrestacao = 920.50m,
                        ValorTotalDesejado = 5000m,
                        ValorTotalCredito = 5523m
                    }
                }
            };

            return Task.FromResult(resultado);
        }
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarVolumeSimuladoDiarioComDadosCorretos()
    {
        // Arrange
        var repository = new ProdutoRepositoryFake();
        var useCase = new ObterVolumeDiarioUseCase(repository);
        var dataReferencia = new DateOnly(2025, 8, 20);

        // Act
        var resultado = await useCase.ExecutarAsync(dataReferencia);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("2025-08-20", resultado.DataReferencia);
        Assert.Equal(2, resultado.Simulacoes.Count);

        var produtoA = resultado.Simulacoes.First(s => s.CodigoProduto == 1);
        Assert.Equal("Produto A", produtoA.DescricaoProduto);
        Assert.Equal(0.035m, produtoA.TaxaMediaJuro);
        Assert.Equal(850.75m, produtoA.ValorMedioPrestacao);
        Assert.Equal(10000m, produtoA.ValorTotalDesejado);
        Assert.Equal(10209m, produtoA.ValorTotalCredito);

        var produtoB = resultado.Simulacoes.First(s => s.CodigoProduto == 2);
        Assert.Equal("Produto B", produtoB.DescricaoProduto);
        Assert.Equal(0.045m, produtoB.TaxaMediaJuro);
        Assert.Equal(920.50m, produtoB.ValorMedioPrestacao);
        Assert.Equal(5000m, produtoB.ValorTotalDesejado);
        Assert.Equal(5523m, produtoB.ValorTotalCredito);
    }
}
