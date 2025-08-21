using hackathon.Domain.Entities;
using hackathon.Infrastructure.Persistence;
using hackathon.Tests.Integration;

namespace hackathon.Tests.Integration.Repositories;

public class SimulacaoRepositoryIntegrationTests : TestBase, IAsyncLifetime
{
    private SimulacaoRepository _repository;
    private IServiceScope _scope;

    public async Task InitializeAsync()
    {
        await TestDataHelper.SetupTestDatabaseAsync(SqliteConnection);
        
        _scope = CreateScope();
        _repository = _scope.ServiceProvider.GetRequiredService<SimulacaoRepository>();
    }

    public async Task DisposeAsync()
    {
        await TestDataHelper.CleanupTestDataAsync(SqliteConnection);
        _scope?.Dispose();
    }

    [Fact]
    public async Task SalvarAsync_ComSimulacaoValida_DevePersistirNoBanco()
    {
        // Arrange
        var simulacao = new Simulacao(
            Guid.NewGuid().ToString(),
            25000.00m,
            2,
            "Produto 2",
            0.0175m,
            DateTime.UtcNow,
            "{}",
            "{}"
        );

        // Act
        await _repository.SalvarAsync(simulacao);

        // Assert
        var simulacoes = await _repository.ObterPaginadoAsync(1, 10);
        Assert.Contains(simulacoes.Registros, s => s.IdSimulacao == simulacao.Id);
    }

    [Fact]
    public async Task ObterPaginadoAsync_ComDadosExistentes_DeveRetornarPaginacaoCorreta()
    {
        // Arrange
        var pagina = 1;
        var qtdRegistros = 2;

        // Act
        var resultado = await _repository.ObterPaginadoAsync(pagina, qtdRegistros);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(pagina, resultado.Pagina);
        Assert.Equal(qtdRegistros, resultado.QtdRegistrosPagina);
        Assert.True(resultado.QtdRegistros >= 3); // Dados de teste
        Assert.True(resultado.Registros.Count() <= qtdRegistros);
    }

    [Fact]
    public async Task ObterPaginadoAsync_ComPaginaVazia_DeveRetornarListaVazia()
    {
        // Arrange
        var pagina = 999; // Página que não existe
        var qtdRegistros = 10;

        // Act
        var resultado = await _repository.ObterPaginadoAsync(pagina, qtdRegistros);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(pagina, resultado.Pagina);
        Assert.Equal(qtdRegistros, resultado.QtdRegistrosPagina);
        Assert.Empty(resultado.Registros);
    }

    [Fact]
    public async Task ObterPaginadoAsync_ComQuantidadeZero_DeveRetornarListaVazia()
    {
        // Arrange
        var pagina = 1;
        var qtdRegistros = 0;

        // Act
        var resultado = await _repository.ObterPaginadoAsync(pagina, qtdRegistros);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(pagina, resultado.Pagina);
        Assert.Equal(qtdRegistros, resultado.QtdRegistrosPagina);
        Assert.Empty(resultado.Registros);
    }

    [Fact]
    public async Task SalvarAsync_ComSimulacaoDuplicada_DeveSubstituirExistente()
    {
        // Arrange
        var id = "test-duplicate";
        var simulacao1 = new Simulacao(
            id,
            10000.00m,
            1,
            "Produto 1",
            0.015m,
            DateTime.UtcNow,
            "{}",
            "{}"
        );

        var simulacao2 = new Simulacao(
            id,
            20000.00m, // Valor diferente
            1,
            "Produto 1",
            0.015m,
            DateTime.UtcNow,
            "{}",
            "{}"
        );

        // Act
        await _repository.SalvarAsync(simulacao1);
        await _repository.SalvarAsync(simulacao2);

        // Assert
        var simulacoes = await _repository.ObterPaginadoAsync(1, 10);
        var simulacaoSalva = simulacoes.Registros.FirstOrDefault(s => s.IdSimulacao == id);
        
        Assert.NotNull(simulacaoSalva);
        Assert.Equal(20000.00m, simulacaoSalva.ValorDesejado); // Deve ter o valor da segunda simulação
    }

    [Fact]
    public async Task ObterPaginadoAsync_ComFiltros_DeveRespeitarPaginacao()
    {
        // Arrange
        var pagina1 = 1;
        var pagina2 = 2;
        var qtdRegistros = 1;

        // Act
        var resultado1 = await _repository.ObterPaginadoAsync(pagina1, qtdRegistros);
        var resultado2 = await _repository.ObterPaginadoAsync(pagina2, qtdRegistros);

        // Assert
        Assert.NotNull(resultado1);
        Assert.NotNull(resultado2);
        Assert.Equal(pagina1, resultado1.Pagina);
        Assert.Equal(pagina2, resultado2.Pagina);
        Assert.Equal(1, resultado1.Registros.Count());
        Assert.Equal(1, resultado2.Registros.Count());
        
        // As páginas devem ter registros diferentes
        var id1 = resultado1.Registros.First().IdSimulacao;
        var id2 = resultado2.Registros.First().IdSimulacao;
        Assert.NotEqual(id1, id2);
    }
}
