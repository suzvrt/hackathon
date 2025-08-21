using System.Threading.Channels;
using hackathon.Application.Interfaces;
using hackathon.Application.UseCases;
using hackathon.Infrastructure.BackgroundServices;
using hackathon.Infrastructure.Config;
using hackathon.Infrastructure.Events;
using hackathon.Infrastructure.Persistence;
using hackathon.Infrastructure.Services;
using hackathon.Infrastructure.Telemetry;

namespace hackathon.Api.Extensions;

public static class DependencyInjection
{
    public static void AddHackathonServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSettings = new DatabaseSettings
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("String de conexão não encontrada.")
        };

        var sqliteSettings = new SqliteSettings
        {
            DatabasePath = configuration["Sqlite:DatabasePath"] ?? "hackathon.db",
            ConnectionString = configuration["Sqlite:ConnectionString"] ?? "Data Source=hackathon.db;Cache=Shared;"
        };

        var eventHubSettings = new EventHubSettings
        {
            ConnectionString = configuration["EventHub:ConnectionString"]
                ?? throw new InvalidOperationException("String de conexão do EventHub não encontrada.")
        };

        services.AddSingleton(dbSettings);
        services.AddSingleton(sqliteSettings);
        services.AddSingleton(eventHubSettings);

        services.AddSingleton<HybridConnectionFactory>();
        services.AddSingleton<IHybridConnectionFactory>(sp => sp.GetRequiredService<HybridConnectionFactory>());
        services.AddSingleton<ISqliteInitializer, SqliteInitializer>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<ISimulacaoRepository, SimulacaoRepository>();

        services.AddSingleton<IEventPublisher, EventHubPublisher>();

        services.AddSingleton<SimulacaoPersistenceService>();
        services.AddHostedService(sp => sp.GetRequiredService<SimulacaoPersistenceService>());
        services.AddScoped<ISimulacaoPersistenceService>(sp =>
            sp.GetRequiredService<SimulacaoPersistenceService>());

        services.AddScoped<ISimularEmprestimoUseCase, SimularEmprestimoUseCase>();
        services.AddScoped<IObterSimulacoesUseCase, ObterSimulacoesUseCase>();
        services.AddScoped<IObterVolumeDiarioUseCase, ObterVolumeDiarioUseCase>();

        // Serviços de telemetria
        services.AddSingleton<ITelemetriaRepository, TelemetriaRepository>();

        // Canal singleton (SingleReader/MultipleWriters)
        services.AddSingleton(Channel.CreateUnbounded<TelemetriaMessage>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }));

        // Fachada que enfileira e o repositório já existente
        services.AddSingleton<ITelemetriaService, TelemetriaService>();

        // Worker que agrega e persiste
        services.AddHostedService<TelemetriaBackgroundService>();
    }
}