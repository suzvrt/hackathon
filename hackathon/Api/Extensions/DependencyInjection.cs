using hackathon.Application.Interfaces;
using hackathon.Application.UseCases;
using hackathon.Infrastructure.Config;
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

        services.AddSingleton(dbSettings);
        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<SimularEmprestimoUseCase>();
    }
}
