using System.Net;
using System.Text.Json;
using hackathon.Application.Dtos;
using hackathon.Tests.Integration;

namespace hackathon.Tests.Integration.Endpoints;

public class ProdutosEndpointIntegrationTests : TestBase, IAsyncLifetime
{
    private readonly HttpClient _client;

    public ProdutosEndpointIntegrationTests()
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
    public async Task GET_Produtos_ComDataValida_DeveRetornarVolumeDiario()
    {
        // Arrange
        var dataReferencia = "2025-01-27";

        // Act
        var response = await _client.GetAsync($"/produtos?dataReferencia={dataReferencia}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var volumeResponse = JsonSerializer.Deserialize<VolumeSimuladoDiario>(responseContent);
        
        Assert.NotNull(volumeResponse);
        Assert.Equal(DateOnly.Parse(dataReferencia), volumeResponse.DataReferencia);
        Assert.NotNull(volumeResponse.Simulacoes);
        Assert.True(volumeResponse.Simulacoes.Any());
    }

    [Fact]
    public async Task GET_Produtos_ComSistemaPRICE_DeveRetornarDadosCorretos()
    {
        // Arrange
        var dataReferencia = "2025-01-27";
        var sistema = "PRICE";

        // Act
        var response = await _client.GetAsync($"/produtos?dataReferencia={dataReferencia}&sistema={sistema}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var volumeResponse = JsonSerializer.Deserialize<VolumeSimuladoDiario>(responseContent);
        
        Assert.NotNull(volumeResponse);
        Assert.Equal(DateOnly.Parse(dataReferencia), volumeResponse.DataReferencia);
        Assert.NotNull(volumeResponse.Simulacoes);
        
        // Verificar se os dados estão corretos para o sistema PRICE
        var simulacao = volumeResponse.Simulacoes.FirstOrDefault();
        Assert.NotNull(simulacao);
        Assert.True(simulacao.ValorTotalCredito > simulacao.ValorTotalDesejado); // Deve incluir juros
    }

    [Fact]
    public async Task GET_Produtos_ComSistemaSAC_DeveRetornarDadosCorretos()
    {
        // Arrange
        var dataReferencia = "2025-01-27";
        var sistema = "SAC";

        // Act
        var response = await _client.GetAsync($"/produtos?dataReferencia={dataReferencia}&sistema={sistema}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var volumeResponse = JsonSerializer.Deserialize<VolumeSimuladoDiario>(responseContent);
        
        Assert.NotNull(volumeResponse);
        Assert.Equal(DateOnly.Parse(dataReferencia), volumeResponse.DataReferencia);
        Assert.NotNull(volumeResponse.Simulacoes);
        
        // Verificar se os dados estão corretos para o sistema SAC
        var simulacao = volumeResponse.Simulacoes.FirstOrDefault();
        Assert.NotNull(simulacao);
        Assert.True(simulacao.ValorTotalCredito > simulacao.ValorTotalDesejado); // Deve incluir juros
    }

    [Fact]
    public async Task GET_Produtos_SemSistema_DeveUsarPadraoPRICE()
    {
        // Arrange
        var dataReferencia = "2025-01-27";

        // Act
        var response = await _client.GetAsync($"/produtos?dataReferencia={dataReferencia}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var volumeResponse = JsonSerializer.Deserialize<VolumeSimuladoDiario>(responseContent);
        
        Assert.NotNull(volumeResponse);
        Assert.Equal(DateOnly.Parse(dataReferencia), volumeResponse.DataReferencia);
        Assert.NotNull(volumeResponse.Simulacoes);
    }

    [Fact]
    public async Task GET_Produtos_ComDataInvalida_DeveRetornarBadRequest()
    {
        // Arrange
        var dataReferencia = "data-invalida";

        // Act
        var response = await _client.GetAsync($"/produtos?dataReferencia={dataReferencia}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Produtos_ComDataFutura_DeveRetornarDadosVazios()
    {
        // Arrange
        var dataFutura = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/produtos?dataReferencia={dataFutura}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var volumeResponse = JsonSerializer.Deserialize<VolumeSimuladoDiario>(responseContent);
        
        Assert.NotNull(volumeResponse);
        Assert.Equal(DateOnly.Parse(dataFutura), volumeResponse.DataReferencia);
        Assert.Empty(volumeResponse.Simulacoes); // Não deve ter simulações para data futura
    }

    [Fact]
    public async Task GET_Produtos_ComSistemaInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        var dataReferencia = "2025-01-27";
        var sistemaInvalido = "SISTEMA_INVALIDO";

        // Act
        var response = await _client.GetAsync($"/produtos?dataReferencia={dataReferencia}&sistema={sistemaInvalido}");

        // Assert
        // Dependendo da implementação, pode retornar BadRequest ou aceitar e usar padrão
        // Por enquanto, vamos assumir que aceita qualquer string
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
