using hackathon.Application.Interfaces;
using hackathon.Application.UseCases;
using hackathon.Infrastructure.BackgroundServices;
using hackathon.Infrastructure.Config;
using hackathon.Infrastructure.Events;
using hackathon.Infrastructure.Persistence;
using hackathon.Infrastructure.Services;
using hackathon.Infrastructure.Telemetria;
using System.Threading.Channels;

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
            ConnectionString = configuration["Sqlite:ConnectionString"] ?? "Data Source=hackathon.db;Cache=Shared;Mode=Wal;"
        };

        var eventHubSettings = new EventHubSettings
        {
            ConnectionString = configuration["EventHub:ConnectionString"]
                ?? throw new InvalidOperationException("String de conexão do EventHub não encontrada.")
        };

        services.AddMemoryCache();

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

        // Serviços de Telemetria
        services.AddSingleton<ITelemetriaService, TelemetriaService>();
        services.AddSingleton<Channel<TelemetriaMessage>>(sp =>
        {
            var options = new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };
            return Channel.CreateBounded<TelemetriaMessage>(options);
        });
        services.AddScoped<ITelemetriaRepository, TelemetriaRepository>();
        services.AddHostedService<TelemetriaBackgroundService>();
    }
}