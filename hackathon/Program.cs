using hackathon.Api.Endpoints;
using hackathon.Api.Extensions;
using hackathon.Api.Serialization;
using hackathon.Infrastructure.Persistence;
using hackathon.Api.Middleware;
using Dapper;

var builder = WebApplication.CreateSlimBuilder(args);

// Configuração de serviços
builder.Services.AddHackathonServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

// Inicializar banco de dados SQLite e warm-up do SqlServer
using (var scope = app.Services.CreateScope())
{
    var sqliteInitializer = scope.ServiceProvider.GetRequiredService<ISqliteInitializer>();
    await sqliteInitializer.InitializeAsync();

    var hybridFactory = scope.ServiceProvider.GetRequiredService<IHybridConnectionFactory>();
    using var conn = hybridFactory.CreateConnection(DatabaseType.SqlServer);
    await conn.ExecuteAsync("SELECT 1");
}

// Mapeamento de endpoints
app.MapSimulacoes();
app.MapProdutos();
app.MapTelemetria();

// Adicionar middleware de telemetria
app.UseTelemetria();

app.Run();
