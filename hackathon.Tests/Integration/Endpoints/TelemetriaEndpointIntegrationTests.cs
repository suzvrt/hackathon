using System.Net;
using System.Text.Json;
using hackathon.Application.Dtos;
using hackathon.Tests.Integration;

namespace hackathon.Tests.Integration.Endpoints;

public class TelemetriaEndpointIntegrationTests : TestBase, IAsyncLifetime
{
    private readonly HttpClient _client;

    public TelemetriaEndpointIntegrationTests()
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
    public async Task GET_Telemetria_SemDataReferencia_DeveRetornarDadosDoDiaAtual()
    {
        // Arrange
        var hoje = DateTime.Today.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync("/telemetria");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var telemetriaResponse = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetriaResponse);
        Assert.Equal(DateOnly.Parse(hoje), telemetriaResponse.DataReferencia);
        Assert.NotNull(telemetriaResponse.ListaEndpoints);
    }

    [Fact]
    public async Task GET_Telemetria_ComDataEspecifica_DeveRetornarDadosDaData()
    {
        // Arrange
        var dataReferencia = "2025-01-27";

        // Act
        var response = await _client.GetAsync($"/telemetria?dataReferencia={dataReferencia}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var telemetriaResponse = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetriaResponse);
        Assert.Equal(DateOnly.Parse(dataReferencia), telemetriaResponse.DataReferencia);
        Assert.NotNull(telemetriaResponse.ListaEndpoints);
        Assert.True(telemetriaResponse.ListaEndpoints.Any());
    }

    [Fact]
    public async Task GET_Telemetria_ComDataInvalida_DeveRetornarBadRequest()
    {
        // Arrange
        var dataInvalida = "data-invalida";

        // Act
        var response = await _client.GetAsync($"/telemetria?dataReferencia={dataInvalida}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_Telemetria_ComDataFutura_DeveRetornarDadosVazios()
    {
        // Arrange
        var dataFutura = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/telemetria?dataReferencia={dataFutura}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var telemetriaResponse = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetriaResponse);
        Assert.Equal(DateOnly.Parse(dataFutura), telemetriaResponse.DataReferencia);
        Assert.Empty(telemetriaResponse.ListaEndpoints); // Não deve ter telemetria para data futura
    }

    [Fact]
    public async Task GET_Telemetria_DeveRetornarMetricasCorretas()
    {
        // Arrange
        var dataReferencia = "2025-01-27";

        // Act
        var response = await _client.GetAsync($"/telemetria?dataReferencia={dataReferencia}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var telemetriaResponse = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetriaResponse);
        Assert.Equal(DateOnly.Parse(dataReferencia), telemetriaResponse.DataReferencia);
        
        var simulacaoEndpoint = telemetriaResponse.ListaEndpoints.FirstOrDefault(e => e.NomeApi == "Simulacao");
        Assert.NotNull(simulacaoEndpoint);
        Assert.Equal(150, simulacaoEndpoint.QtdRequisicoes);
        Assert.Equal(45, simulacaoEndpoint.TempoMedio);
        Assert.Equal(12, simulacaoEndpoint.TempoMinimo);
        Assert.Equal(120, simulacaoEndpoint.TempoMaximo);
        Assert.Equal(0.985, simulacaoEndpoint.PercentualSucesso);
    }

    [Fact]
    public async Task GET_Telemetria_DeveRetornarTodosOsEndpoints()
    {
        // Arrange
        var dataReferencia = "2025-01-27";

        // Act
        var response = await _client.GetAsync($"/telemetria?dataReferencia={dataReferencia}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var telemetriaResponse = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetriaResponse);
        
        var endpointNames = telemetriaResponse.ListaEndpoints.Select(e => e.NomeApi).ToList();
        Assert.Contains("Simulacao", endpointNames);
        Assert.Contains("Produtos", endpointNames);
    }

    [Fact]
    public async Task GET_Telemetria_ComDataSemDados_DeveRetornarListaVazia()
    {
        // Arrange
        var dataSemDados = "2024-01-01"; // Data anterior aos dados de teste

        // Act
        var response = await _client.GetAsync($"/telemetria?dataReferencia={dataSemDados}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var telemetriaResponse = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetriaResponse);
        Assert.Equal(DateOnly.Parse(dataSemDados), telemetriaResponse.DataReferencia);
        Assert.Empty(telemetriaResponse.ListaEndpoints);
    }
}
