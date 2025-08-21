using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using hackathon.Infrastructure.Config;
using hackathon.Infrastructure.Persistence;
using hackathon.Infrastructure.Services;
using hackathon.Application.UseCases;
using hackathon.Application.Interfaces;

namespace hackathon.Tests.Integration;

public static class TestConfiguration
{
    public static IServiceCollection ConfigureTestServices(IServiceCollection services)
    {
        // Configurações de teste
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:;Cache=Shared;",
                ["Sqlite:DatabasePath"] = ":memory:",
                ["Sqlite:ConnectionString"] = "Data Source=:memory:;Cache=Shared;",
                ["EventHub:ConnectionString"] = "fake-connection-string-for-tests",
                ["EventHub:EntityPath"] = "test-entity"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Configurar SQLite em memória
        services.Configure<SqliteSettings>(configuration.GetSection("Sqlite"));
        services.Configure<DatabaseSettings>(configuration.GetSection("ConnectionStrings"));
        services.Configure<EventHubSettings>(configuration.GetSection("EventHub"));

        // Configurar fábrica de conexão híbrida para testes
        services.AddSingleton<IHybridConnectionFactory, HybridConnectionFactory>();

        // Configurar repositórios
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<ISimulacaoRepository, SimulacaoRepository>();
        services.AddScoped<ITelemetriaRepository, TelemetriaRepository>();

        // Configurar serviços
        services.AddSingleton<ITelemetriaService, TelemetriaService>();
        services.AddScoped<IEventPublisher, EventHubPublisher>();

        // Configurar casos de uso
        services.AddScoped<ISimularEmprestimoUseCase, SimularEmprestimoUseCase>();
        services.AddScoped<IObterSimulacoesUseCase, ObterSimulacoesUseCase>();
        services.AddScoped<IObterVolumeDiarioUseCase, ObterVolumeDiarioUseCase>();

        // Configurar serviços de background (desabilitados para testes)
        services.AddScoped<ISimulacaoPersistenceService, SimulacaoPersistenceService>();

        return services;
    }

    public static IHostBuilder ConfigureTestHost(IHostBuilder builder)
    {
        return builder.ConfigureServices(ConfigureTestServices);
    }
}
