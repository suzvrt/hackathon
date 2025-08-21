using System.Net;
using System.Text;
using System.Text.Json;
using hackathon.Application.Dtos;
using hackathon.Tests.Integration;

namespace hackathon.Tests.Integration.Endpoints;

public class SimulacoesEndpointIntegrationTests : TestBase, IAsyncLifetime
{
    private readonly HttpClient _client;

    public SimulacoesEndpointIntegrationTests()
    {
        _client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await TestDataHelper.SetupTestDatabaseAsync(SqliteConnection);
    }

    public async Task DisposeAsync()
    {
        await TestDataHelper.CleanupTestDataAsync(SqliteConnection);
    }

    [Fact]
    public async Task POST_Simulacoes_ComValoresValidos_DeveRetornarSimulacao()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 15000.00m,
            Prazo = 24
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/simulacoes", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var simulacaoResponse = JsonSerializer.Deserialize<SimularResponse>(responseContent);
        
        Assert.NotNull(simulacaoResponse);
        Assert.Equal(1, simulacaoResponse.CodigoProduto);
        Assert.Equal("Produto 1", simulacaoResponse.DescricaoProduto);
        Assert.Equal(0.015m, simulacaoResponse.TaxaJuros);
        Assert.NotNull(simulacaoResponse.Sac);
        Assert.NotNull(simulacaoResponse.Price);
    }

    [Fact]
    public async Task POST_Simulacoes_ComValoresInvalidos_DeveRetornarNotFound()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 999999.00m, // Valor muito alto
            Prazo = 999 // Prazo muito alto
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/simulacoes", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Nenhum produto compatível encontrado", responseContent);
    }

    [Fact]
    public async Task GET_Simulacoes_DeveRetornarListaPaginada()
    {
        // Arrange
        var expectedCount = 3; // Dados de teste

        // Act
        var response = await _client.GetAsync("/simulacoes?pagina=1&qtdRegistrosPagina=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var simulacoesResponse = JsonSerializer.Deserialize<PaginacaoResultado<SimulacaoResumo>>(responseContent);
        
        Assert.NotNull(simulacoesResponse);
        Assert.Equal(1, simulacoesResponse.Pagina);
        Assert.Equal(expectedCount, simulacoesResponse.QtdRegistros);
        Assert.Equal(10, simulacoesResponse.QtdRegistrosPagina);
        Assert.Equal(expectedCount, simulacoesResponse.Registros.Count());
    }

    [Fact]
    public async Task GET_Simulacoes_ComPaginacaoCustomizada_DeveRespeitarParametros()
    {
        // Arrange
        var pagina = 1;
        var qtdRegistros = 2;

        // Act
        var response = await _client.GetAsync($"/simulacoes?pagina={pagina}&qtdRegistrosPagina={qtdRegistros}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var simulacoesResponse = JsonSerializer.Deserialize<PaginacaoResultado<SimulacaoResumo>>(responseContent);
        
        Assert.NotNull(simulacoesResponse);
        Assert.Equal(pagina, simulacoesResponse.Pagina);
        Assert.Equal(qtdRegistros, simulacoesResponse.QtdRegistrosPagina);
        Assert.True(simulacoesResponse.Registros.Count() <= qtdRegistros);
    }

    [Fact]
    public async Task POST_Simulacoes_DevePersistirSimulacao()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 20000.00m,
            Prazo = 36
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Criar simulação
        var createResponse = await _client.PostAsync("/simulacoes", content);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        // Act - Buscar simulações
        var listResponse = await _client.GetAsync("/simulacoes?pagina=1&qtdRegistrosPagina=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        
        var responseContent = await listResponse.Content.ReadAsStringAsync();
        var simulacoesResponse = JsonSerializer.Deserialize<PaginacaoResultado<SimulacaoResumo>>(responseContent);
        
        Assert.NotNull(simulacoesResponse);
        Assert.True(simulacoesResponse.QtdRegistros >= 4); // 3 originais + 1 nova
        Assert.True(simulacoesResponse.Registros.Any(s => s.ValorDesejado == 20000.00m));
    }
}
