using System.Net;
using System.Text;
using System.Text.Json;
using hackathon.Application.Dtos;
using hackathon.Tests.Integration;

namespace hackathon.Tests.Integration.Middleware;

public class TelemetryMiddlewareIntegrationTests : TestBase, IAsyncLifetime
{
    private readonly HttpClient _client;

    public TelemetryMiddlewareIntegrationTests()
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
    public async Task Middleware_DeveCapturarTelemetriaAutomaticamente()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 15000.00m,
            Prazo = 24
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Fazer algumas requisições para gerar telemetria
        await _client.PostAsync("/simulacoes", content);
        await _client.PostAsync("/simulacoes", content);
        await _client.GetAsync("/produtos?dataReferencia=2025-01-27");
        await _client.GetAsync("/telemetria");

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(200);

        // Assert - Verificar se a telemetria foi capturada
        var telemetriaResponse = await _client.GetAsync("/telemetria");
        Assert.Equal(HttpStatusCode.OK, telemetriaResponse.StatusCode);
        
        var responseContent = await telemetriaResponse.Content.ReadAsStringAsync();
        var telemetria = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetria);
        Assert.True(telemetria.ListaEndpoints.Any());
        
        // Deve ter telemetria para os endpoints chamados
        var simulacoesEndpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == "Simulacao");
        var produtosEndpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == "Produtos");
        
        Assert.NotNull(simulacoesEndpoint);
        Assert.NotNull(produtosEndpoint);
        Assert.True(simulacoesEndpoint.QtdRequisicoes >= 2);
        Assert.True(produtosEndpoint.QtdRequisicoes >= 1);
    }

    [Fact]
    public async Task Middleware_DeveCapturarTempoDeResposta()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 10000.00m,
            Prazo = 12
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        await _client.PostAsync("/simulacoes", content);

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(200);

        // Assert
        var telemetriaResponse = await _client.GetAsync("/telemetria");
        Assert.Equal(HttpStatusCode.OK, telemetriaResponse.StatusCode);
        
        var responseContent = await telemetriaResponse.Content.ReadAsStringAsync();
        var telemetria = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetria);
        
        var simulacoesEndpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == "Simulacao");
        Assert.NotNull(simulacoesEndpoint);
        Assert.True(simulacoesEndpoint.TempoMedio > 0);
        Assert.True(simulacoesEndpoint.TempoMinimo > 0);
        Assert.True(simulacoesEndpoint.TempoMaximo > 0);
    }

    [Fact]
    public async Task Middleware_DeveCapturarTaxaDeSucesso()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 999999.00m, // Valor inválido para gerar erro
            Prazo = 999
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Fazer requisições válidas e inválidas
        await _client.PostAsync("/simulacoes", content); // Deve falhar
        await _client.GetAsync("/produtos?dataReferencia=2025-01-27"); // Deve funcionar
        await _client.GetAsync("/telemetria"); // Deve funcionar

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(200);

        // Assert
        var telemetriaResponse = await _client.GetAsync("/telemetria");
        Assert.Equal(HttpStatusCode.OK, telemetriaResponse.StatusCode);
        
        var responseContent = await telemetriaResponse.Content.ReadAsStringAsync();
        var telemetria = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetria);
        
        // Verificar se a taxa de sucesso foi calculada corretamente
        var simulacoesEndpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == "Simulacao");
        Assert.NotNull(simulacoesEndpoint);
        Assert.True(simulacoesEndpoint.PercentualSucesso >= 0);
        Assert.True(simulacoesEndpoint.PercentualSucesso <= 1);
    }

    [Fact]
    public async Task Middleware_DeveCapturarTelemetriaDeTodosOsEndpoints()
    {
        // Arrange
        var endpoints = new[]
        {
            "/simulacoes",
            "/produtos?dataReferencia=2025-01-27",
            "/telemetria"
        };

        // Act - Chamar todos os endpoints
        foreach (var endpoint in endpoints)
        {
            if (endpoint.StartsWith("/simulacoes"))
            {
                var request = new SimulacaoRequest
                {
                    ValorDesejado = 15000.00m,
                    Prazo = 24
                };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _client.PostAsync(endpoint, content);
            }
            else
            {
                await _client.GetAsync(endpoint);
            }
        }

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(200);

        // Assert
        var telemetriaResponse = await _client.GetAsync("/telemetria");
        Assert.Equal(HttpStatusCode.OK, telemetriaResponse.StatusCode);
        
        var responseContent = await telemetriaResponse.Content.ReadAsStringAsync();
        var telemetria = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetria);
        Assert.True(telemetria.ListaEndpoints.Any());
        
        // Deve ter telemetria para todos os endpoints chamados
        var endpointNames = telemetria.ListaEndpoints.Select(e => e.NomeApi).ToList();
        Assert.Contains("Simulacao", endpointNames);
        Assert.Contains("Produtos", endpointNames);
        Assert.Contains("Telemetria", endpointNames);
    }

    [Fact]
    public async Task Middleware_DeveCapturarTelemetriaEmTempoReal()
    {
        // Arrange
        var request = new SimulacaoRequest
        {
            ValorDesejado = 20000.00m,
            Prazo = 36
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act - Fazer requisição e verificar telemetria imediatamente
        await _client.PostAsync("/simulacoes", content);
        
        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(200);

        // Assert
        var telemetriaResponse = await _client.GetAsync("/telemetria");
        Assert.Equal(HttpStatusCode.OK, telemetriaResponse.StatusCode);
        
        var responseContent = await telemetriaResponse.Content.ReadAsStringAsync();
        var telemetria = JsonSerializer.Deserialize<TelemetriaResponse>(responseContent);
        
        Assert.NotNull(telemetria);
        
        var simulacoesEndpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == "Simulacao");
        Assert.NotNull(simulacoesEndpoint);
        Assert.True(simulacoesEndpoint.QtdRequisicoes > 0);
    }
}
