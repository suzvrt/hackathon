using hackathon.Domain.Entities;
using hackathon.Infrastructure.Persistence;
using hackathon.Tests.Integration;

namespace hackathon.Tests.Integration.Repositories;

public class ProdutoRepositoryIntegrationTests : TestBase, IAsyncLifetime
{
    private ProdutoRepository _repository;
    private IServiceScope _scope;

    public async Task InitializeAsync()
    {
        await TestDataHelper.SetupTestDatabaseAsync(SqliteConnection);
        
        _scope = CreateScope();
        _repository = _scope.ServiceProvider.GetRequiredService<ProdutoRepository>();
    }

    public async Task DisposeAsync()
    {
        await TestDataHelper.CleanupTestDataAsync(SqliteConnection);
        _scope?.Dispose();
    }

    [Fact]
    public async Task ObterProdutosCompativeisAsync_ComValoresValidos_DeveRetornarProdutos()
    {
        // Arrange
        var valor = 15000.00m;
        var prazo = 24;

        // Act
        var produtos = await _repository.ObterProdutosCompativeisAsync(valor, prazo);

        // Assert
        Assert.NotNull(produtos);
        Assert.True(produtos.Any());
        
        foreach (var produto in produtos)
        {
            Assert.True(produto.ValorMinimo <= valor);
            Assert.True(produto.ValorMaximo >= valor);
            Assert.True(produto.MinimoMeses <= prazo);
            Assert.True(produto.MaximoMeses >= prazo);
        }
    }

    [Fact]
    public async Task ObterProdutosCompativeisAsync_ComValorBaixo_DeveRetornarProdutosCompatíveis()
    {
        // Arrange
        var valor = 5000.00m;
        var prazo = 12;

        // Act
        var produtos = await _repository.ObterProdutosCompativeisAsync(valor, prazo);

        // Assert
        Assert.NotNull(produtos);
        Assert.True(produtos.Any());
        
        // Deve retornar produtos que aceitam valores baixos
        var produtoCompativel = produtos.FirstOrDefault(p => p.ValorMinimo <= valor);
        Assert.NotNull(produtoCompativel);
    }

    [Fact]
    public async Task ObterProdutosCompativeisAsync_ComValorAlto_DeveRetornarProdutosCompatíveis()
    {
        // Arrange
        var valor = 75000.00m;
        var prazo = 60;

        // Act
        var produtos = await _repository.ObterProdutosCompativeisAsync(valor, prazo);

        // Assert
        Assert.NotNull(produtos);
        Assert.True(produtos.Any());
        
        // Deve retornar produtos que aceitam valores altos
        var produtoCompativel = produtos.FirstOrDefault(p => p.ValorMaximo >= valor);
        Assert.NotNull(produtoCompativel);
    }

    [Fact]
    public async Task ObterProdutosCompativeisAsync_ComValoresIncompatíveis_DeveRetornarListaVazia()
    {
        // Arrange
        var valor = 999999.00m; // Valor muito alto
        var prazo = 999; // Prazo muito alto

        // Act
        var produtos = await _repository.ObterProdutosCompativeisAsync(valor, prazo);

        // Assert
        Assert.NotNull(produtos);
        Assert.Empty(produtos);
    }

    [Fact]
    public async Task ObterVolumeSimuladoPorDiaAsync_ComDataValida_DeveRetornarVolume()
    {
        // Arrange
        var dataReferencia = DateOnly.Parse("2025-01-27");
        var sistema = "PRICE";

        // Act
        var volume = await _repository.ObterVolumeSimuladoPorDiaAsync(dataReferencia, sistema);

        // Assert
        Assert.NotNull(volume);
        Assert.Equal(dataReferencia, volume.DataReferencia);
        Assert.NotNull(volume.Simulacoes);
        Assert.True(volume.Simulacoes.Any());
    }

    [Fact]
    public async Task ObterVolumeSimuladoPorDiaAsync_ComSistemaSAC_DeveRetornarVolume()
    {
        // Arrange
        var dataReferencia = DateOnly.Parse("2025-01-27");
        var sistema = "SAC";

        // Act
        var volume = await _repository.ObterVolumeSimuladoPorDiaAsync(dataReferencia, sistema);

        // Assert
        Assert.NotNull(volume);
        Assert.Equal(dataReferencia, volume.DataReferencia);
        Assert.NotNull(volume.Simulacoes);
        Assert.True(volume.Simulacoes.Any());
    }

    [Fact]
    public async Task ObterVolumeSimuladoPorDiaAsync_ComDataSemDados_DeveRetornarVolumeVazio()
    {
        // Arrange
        var dataReferencia = DateOnly.Parse("2024-01-01"); // Data sem dados
        var sistema = "PRICE";

        // Act
        var volume = await _repository.ObterVolumeSimuladoPorDiaAsync(dataReferencia, sistema);

        // Assert
        Assert.NotNull(volume);
        Assert.Equal(dataReferencia, volume.DataReferencia);
        Assert.Empty(volume.Simulacoes);
    }

    [Fact]
    public async Task ObterVolumeSimuladoPorDiaAsync_ComSistemaNulo_DeveUsarPadrao()
    {
        // Arrange
        var dataReferencia = DateOnly.Parse("2025-01-27");
        string? sistema = null;

        // Act
        var volume = await _repository.ObterVolumeSimuladoPorDiaAsync(dataReferencia, sistema);

        // Assert
        Assert.NotNull(volume);
        Assert.Equal(dataReferencia, volume.DataReferencia);
        Assert.NotNull(volume.Simulacoes);
        Assert.True(volume.Simulacoes.Any());
    }

    [Fact]
    public async Task ObterVolumeSimuladoPorDiaAsync_DeveCalcularMetricasCorretas()
    {
        // Arrange
        var dataReferencia = DateOnly.Parse("2025-01-27");
        var sistema = "PRICE";

        // Act
        var volume = await _repository.ObterVolumeSimuladoPorDiaAsync(dataReferencia, sistema);

        // Assert
        Assert.NotNull(volume);
        Assert.True(volume.Simulacoes.Any());
        
        var simulacao = volume.Simulacoes.First();
        Assert.True(simulacao.ValorTotalCredito > simulacao.ValorTotalDesejado); // Deve incluir juros
        Assert.True(simulacao.ValorMedioPrestacao > 0);
        Assert.True(simulacao.TaxaMediaJuro > 0);
    }
}
