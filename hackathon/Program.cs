using hackathon.Api.Endpoints;
using hackathon.Api.Extensions;
using hackathon.Api.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

// Configuração de serviços
builder.Services.AddHackathonServices(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

// Mapeamento de endpoints
app.MapSimulacoes();
app.MapProdutos();

app.Run();
