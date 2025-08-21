using hackathon.Application.Dtos;
using hackathon.Infrastructure.Services;
using hackathon.Tests.Integration;

namespace hackathon.Tests.Integration.Services;

public class TelemetriaServiceIntegrationTests : TestBase, IAsyncLifetime
{
    private TelemetriaService _service;
    private IServiceScope _scope;

    public async Task InitializeAsync()
    {
        await TestDataHelper.SetupTestDatabaseAsync(SqliteConnection);
        
        _scope = CreateScope();
        _service = _scope.ServiceProvider.GetRequiredService<TelemetriaService>();
    }

    public async Task DisposeAsync()
    {
        await TestDataHelper.CleanupTestDataAsync(SqliteConnection);
        _scope?.Dispose();
    }

    [Fact]
    public async Task RegistrarRequisicao_ComDadosValidos_DeveIncrementarContador()
    {
        // Arrange
        var nomeApi = "TesteAPI";
        var tempoResposta = 150;
        var sucesso = true;

        // Act
        _service.RegistrarRequisicao(nomeApi, tempoResposta, sucesso);
        _service.RegistrarRequisicao(nomeApi, 200, true);
        _service.RegistrarRequisicao(nomeApi, 100, false);

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(100);

        // Assert
        var telemetria = await _service.ObterTelemetriaAsync(DateTime.Today);
        Assert.NotNull(telemetria);
        
        var endpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == nomeApi);
        Assert.NotNull(endpoint);
        Assert.Equal(3, endpoint.QtdRequisicoes);
        Assert.Equal(2, endpoint.QtdSucessos); // 2 sucessos, 1 falha
    }

    [Fact]
    public async Task RegistrarRequisicao_ComMúltiplasAPIs_DeveSepararMetricas()
    {
        // Arrange
        var api1 = "API1";
        var api2 = "API2";

        // Act
        _service.RegistrarRequisicao(api1, 100, true);
        _service.RegistrarRequisicao(api1, 150, true);
        _service.RegistrarRequisicao(api2, 200, true);
        _service.RegistrarRequisicao(api2, 250, false);

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(100);

        // Assert
        var telemetria = await _service.ObterTelemetriaAsync(DateTime.Today);
        Assert.NotNull(telemetria);
        
        var endpoint1 = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == api1);
        var endpoint2 = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == api2);
        
        Assert.NotNull(endpoint1);
        Assert.NotNull(endpoint2);
        Assert.Equal(2, endpoint1.QtdRequisicoes);
        Assert.Equal(2, endpoint2.QtdRequisicoes);
        Assert.Equal(2, endpoint1.QtdSucessos);
        Assert.Equal(1, endpoint2.QtdSucessos);
    }

    [Fact]
    public async Task ObterTelemetriaAsync_ComDataEspecifica_DeveRetornarDadosCorretos()
    {
        // Arrange
        var dataReferencia = DateTime.Today;
        var nomeApi = "TesteData";

        // Act
        _service.RegistrarRequisicao(nomeApi, 100, true);
        _service.RegistrarRequisicao(nomeApi, 200, true);

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(100);

        // Assert
        var telemetria = await _service.ObterTelemetriaAsync(dataReferencia);
        Assert.NotNull(telemetria);
        Assert.Equal(DateOnly.FromDateTime(dataReferencia), telemetria.DataReferencia);
        
        var endpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == nomeApi);
        Assert.NotNull(endpoint);
        Assert.Equal(2, endpoint.QtdRequisicoes);
    }

    [Fact]
    public async Task ObterTelemetriaAsync_ComDataSemDados_DeveRetornarListaVazia()
    {
        // Arrange
        var dataSemDados = DateTime.Today.AddDays(-30);

        // Act
        var telemetria = await _service.ObterTelemetriaAsync(dataSemDados);

        // Assert
        Assert.NotNull(telemetria);
        Assert.Equal(DateOnly.FromDateTime(dataSemDados), telemetria.DataReferencia);
        Assert.Empty(telemetria.ListaEndpoints);
    }

    [Fact]
    public async Task RegistrarRequisicao_ComTemposExtremos_DeveCalcularMinMaxCorretamente()
    {
        // Arrange
        var nomeApi = "TesteExtremos";

        // Act
        _service.RegistrarRequisicao(nomeApi, 50, true);   // Mínimo
        _service.RegistrarRequisicao(nomeApi, 100, true);  // Médio
        _service.RegistrarRequisicao(nomeApi, 500, true);  // Máximo

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(100);

        // Assert
        var telemetria = await _service.ObterTelemetriaAsync(DateTime.Today);
        Assert.NotNull(telemetria);
        
        var endpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == nomeApi);
        Assert.NotNull(endpoint);
        Assert.Equal(3, endpoint.QtdRequisicoes);
        Assert.Equal(50, endpoint.TempoMinimo);
        Assert.Equal(500, endpoint.TempoMaximo);
        Assert.True(endpoint.TempoMedio > 0);
    }

    [Fact]
    public async Task RegistrarRequisicao_ComFalhas_DeveCalcularPercentualSucesso()
    {
        // Arrange
        var nomeApi = "TesteFalhas";

        // Act
        _service.RegistrarRequisicao(nomeApi, 100, true);   // Sucesso
        _service.RegistrarRequisicao(nomeApi, 150, true);   // Sucesso
        _service.RegistrarRequisicao(nomeApi, 200, false);  // Falha
        _service.RegistrarRequisicao(nomeApi, 250, false);  // Falha

        // Aguardar um pouco para o descarregamento automático
        await Task.Delay(100);

        // Assert
        var telemetria = await _service.ObterTelemetriaAsync(DateTime.Today);
        Assert.NotNull(telemetria);
        
        var endpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == nomeApi);
        Assert.NotNull(endpoint);
        Assert.Equal(4, endpoint.QtdRequisicoes);
        Assert.Equal(2, endpoint.QtdSucessos);
        Assert.Equal(0.5, endpoint.PercentualSucesso); // 50% de sucesso
    }

    [Fact]
    public async Task DescarregarTelemetria_DevePersistirDadosNoBanco()
    {
        // Arrange
        var nomeApi = "TestePersistencia";

        // Act
        _service.RegistrarRequisicao(nomeApi, 100, true);
        _service.RegistrarRequisicao(nomeApi, 150, true);

        // Forçar descarregamento
        await _service.DescarregarTelemetriaAsync();

        // Assert
        var telemetria = await _service.ObterTelemetriaAsync(DateTime.Today);
        Assert.NotNull(telemetria);
        
        var endpoint = telemetria.ListaEndpoints.FirstOrDefault(e => e.NomeApi == nomeApi);
        Assert.NotNull(endpoint);
        Assert.Equal(2, endpoint.QtdRequisicoes);
    }
}
