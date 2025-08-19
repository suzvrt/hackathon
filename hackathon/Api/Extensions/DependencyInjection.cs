using hackathon.Application.Interfaces;
using hackathon.Application.UseCases;
using hackathon.Infrastructure.BackgroundServices;
using hackathon.Infrastructure.Config;
using hackathon.Infrastructure.Events;
using hackathon.Infrastructure.Persistence;

namespace hackathon.Api.Extensions;

public static class DependencyInjection
{
    public static void AddHackathonServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSettings = new DatabaseSettings
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string não encontrada.")
        };

        var eventHubSettings = new EventHubSettings
        {
            ConnectionString = configuration["EventHub:ConnectionString"] ?? throw new InvalidOperationException("EventHub connection string não encontrada."),
            EventHubName = configuration["EventHub:Name"] ?? throw new InvalidOperationException("EventHub name não encontrado.")
        };

        services.AddSingleton(eventHubSettings);
        services.AddSingleton(dbSettings);
        services.AddSingleton<SimulacaoPersistenceService>();
        services.AddHostedService(sp => sp.GetRequiredService<SimulacaoPersistenceService>());
        services.AddScoped<ISimulacaoPersistenceService>(sp => 
            sp.GetRequiredService<SimulacaoPersistenceService>());
        services.AddSingleton<IEventPublisher, EventHubPublisher>();
        services.AddScoped<ISimulacaoRepository, SimulacaoRepository>();
        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<SimularEmprestimoUseCase>();
    }
}
