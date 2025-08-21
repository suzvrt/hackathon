using hackathon.Application.Dtos;
using hackathon.Application.UseCases;
using hackathon.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace hackathon.Tests.Integration.UseCases;

public class SimularEmprestimoUseCaseIntegrationTests : TestBase, IAsyncLifetime
{
    private SimularEmprestimoUseCase _useCase;
    private IServiceScope _scope;

    public async Task InitializeAsync()
    {
        await TestDataHelper.SetupTestDatabaseAsync(SqliteConnection);
        
        _scope = CreateScope();
        _useCase = _scope.ServiceProvider.GetRequiredService<SimularEmprestimoUseCase>();
    }

    public async Task DisposeAsync()
    {
        await TestDataHelper.CleanupTestDataAsync(SqliteConnection);
        _scope?.Dispose();
    }

    [Fact]
    public async Task ExecutarAsync_ComValoresValidos_DeveRetornarSimulacao()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 15000.00m,
            Prazo = 24
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.CodigoProduto);
        Assert.Equal("Produto 1", resultado.DescricaoProduto);
        Assert.Equal(0.015m, resultado.TaxaJuros);
        Assert.NotNull(resultado.Sac);
        Assert.NotNull(resultado.Price);
        Assert.Equal(request.ValorDesejado, resultado.Sac.ValorDesejado);
        Assert.Equal(request.Prazo, resultado.Sac.Prazo);
        Assert.Equal(request.ValorDesejado, resultado.Price.ValorDesejado);
        Assert.Equal(request.Prazo, resultado.Price.Prazo);
    }

    [Fact]
    public async Task ExecutarAsync_ComValorBaixo_DeveRetornarProdutoCompatível()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 5000.00m,
            Prazo = 12
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.CodigoProduto > 0);
        Assert.NotNull(resultado.DescricaoProduto);
        Assert.True(resultado.TaxaJuros > 0);
        Assert.NotNull(resultado.Sac);
        Assert.NotNull(resultado.Price);
    }

    [Fact]
    public async Task ExecutarAsync_ComValorAlto_DeveRetornarProdutoCompatível()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 75000.00m,
            Prazo = 60
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.True(resultado.CodigoProduto > 0);
        Assert.NotNull(resultado.DescricaoProduto);
        Assert.True(resultado.TaxaJuros > 0);
        Assert.NotNull(resultado.Sac);
        Assert.NotNull(resultado.Price);
    }

    [Fact]
    public async Task ExecutarAsync_ComValoresIncompatíveis_DeveRetornarNull()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 999999.00m, // Valor muito alto
            Prazo = 999 // Prazo muito alto
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ExecutarAsync_ComPrazoMinimo_DeveRetornarProdutoCompatível()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 1000.00m,
            Prazo = 6 // Prazo mínimo do Produto 1
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.CodigoProduto);
        Assert.Equal("Produto 1", resultado.DescricaoProduto);
    }

    [Fact]
    public async Task ExecutarAsync_ComPrazoMaximo_DeveRetornarProdutoCompatível()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 10000.00m,
            Prazo = 240 // Prazo máximo do Produto 3
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.CodigoProduto);
        Assert.Equal("Produto 3", resultado.DescricaoProduto);
    }

    [Fact]
    public async Task ExecutarAsync_DeveCalcularParcelasCorretamente()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 10000.00m,
            Prazo = 12
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.NotNull(resultado.Sac);
        Assert.NotNull(resultado.Price);
        
        // Verificar se as parcelas foram calculadas
        Assert.True(resultado.Sac.Parcelas.Any());
        Assert.True(resultado.Price.Parcelas.Any());
        Assert.Equal(request.Prazo, resultado.Sac.Parcelas.Count());
        Assert.Equal(request.Prazo, resultado.Price.Parcelas.Count());
        
        // Verificar se os valores estão corretos
        var primeiraParcelaSac = resultado.Sac.Parcelas.First();
        var primeiraParcelaPrice = resultado.Price.Parcelas.First();
        
        Assert.True(primeiraParcelaSac.ValorPrestacao > 0);
        Assert.True(primeiraParcelaPrice.ValorPrestacao > 0);
        Assert.True(primeiraParcelaSac.ValorAmortizacao > 0);
        Assert.True(primeiraParcelaPrice.ValorAmortizacao > 0);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarProdutoComMenorTaxa()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 25000.00m,
            Prazo = 36
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        
        // Deve retornar o produto com menor taxa de juros que seja compatível
        // Com base nos dados de teste, deve ser o Produto 2 (taxa 0.0175)
        Assert.Equal(2, resultado.CodigoProduto);
        Assert.Equal(0.0175m, resultado.TaxaJuros);
    }

    [Fact]
    public async Task ExecutarAsync_ComValoresLimite_DeveRetornarProdutoCorreto()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 50000.00m, // Valor máximo do Produto 1
            Prazo = 60 // Prazo máximo do Produto 1
        };

        // Act
        var resultado = await _useCase.ExecutarAsync(request);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.CodigoProduto);
        Assert.Equal("Produto 1", resultado.DescricaoProduto);
    }
}
