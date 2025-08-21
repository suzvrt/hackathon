using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
using hackathon.Infrastructure.Config;
using hackathon.Infrastructure.Persistence;

namespace hackathon.Tests.Integration;

public abstract class TestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly SqliteConnection SqliteConnection;

    protected TestBase()
    {
        // Configurar SQLite em memória para testes
        SqliteConnection = new SqliteConnection("Data Source=:memory:");
        SqliteConnection.Open();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Usar configuração de teste padronizada
                    TestConfiguration.ConfigureTestServices(services);
                    
                    // Configurar serviços específicos do teste se necessário
                    ConfigureTestServices(services);
                });
            });
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Pode ser sobrescrito por testes específicos
    }

    protected IServiceScope CreateScope()
    {
        return Factory.Services.CreateScope();
    }

    protected T GetRequiredService<T>() where T : notnull
    {
        return Factory.Services.GetRequiredService<T>();
    }

    protected T GetRequiredService<T>(IServiceScope scope) where T : notnull
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        SqliteConnection?.Dispose();
        Factory?.Dispose();
    }
}
