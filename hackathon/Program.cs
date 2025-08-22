using hackathon.Api.Endpoints;
using hackathon.Api.Extensions;
using hackathon.Api.Serialization;
using hackathon.Api.Middleware;
using hackathon.Infrastructure.Persistence;
using Dapper;

var builder = WebApplication.CreateSlimBuilder(args);

// Configuração de serviços
builder.Services.AddHackathonServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

// Handler global de exceções => ProblemDetails genérico
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var problem = Results.Problem(
            title: "Não foi possível processar sua solicitação.",
            statusCode: StatusCodes.Status500InternalServerError);
        await problem.ExecuteAsync(context);
    });
});

// Inicializar banco de dados SQLite e warm-up do SqlServer
using (var scope = app.Services.CreateScope())
{
    var sqliteInitializer = scope.ServiceProvider.GetRequiredService<ISqliteInitializer>();
    await sqliteInitializer.InitializeAsync();

    var hybridFactory = scope.ServiceProvider.GetRequiredService<IHybridConnectionFactory>();
    using var conn = hybridFactory.CreateConnection(DatabaseType.SqlServer);
    try { await conn.ExecuteAsync("SELECT 1"); }
    catch (Exception ex) { Console.WriteLine($"Erro no warm-up do banco de dados: {ex.Message}"); }
}

// Adicionar middleware de telemetria
app.UseTelemetria();

// Mapeamento de endpoints
app.MapSimulacoes();
app.MapProdutos();
app.MapTelemetria();

app.Run();
