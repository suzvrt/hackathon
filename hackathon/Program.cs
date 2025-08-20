using hackathon.Api.Endpoints;
using hackathon.Api.Extensions;
using hackathon.Api.Serialization;
using hackathon.Infrastructure.Persistence;
using hackathon.Api.Middleware;

var builder = WebApplication.CreateSlimBuilder(args);

// Configuração de serviços
builder.Services.AddHackathonServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

// Inicializar banco de dados SQLite
using (var scope = app.Services.CreateScope())
{
    var sqliteInitializer = scope.ServiceProvider.GetRequiredService<ISqliteInitializer>();
    await sqliteInitializer.InitializeAsync();
}

// Mapeamento de endpoints
app.MapSimulacoes();
app.MapProdutos();
app.MapTelemetria();

// Adicionar middleware de telemetria
app.UseTelemetria();

app.Run();
