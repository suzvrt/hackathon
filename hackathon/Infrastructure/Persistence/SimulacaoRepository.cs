using System.Text.Json;
using Dapper;
using hackathon.Api.Serialization;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;

namespace hackathon.Infrastructure.Persistence;

public class SimulacaoRepository : ISimulacaoRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public SimulacaoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task SalvarAsync(Simulacao simulacao)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = """
            INSERT INTO dbo.Simulacao (
                Id, CodigoProduto, DescricaoProduto, TaxaJuros, CriadoEm,
                SimulacaoSac, SimulacaoPrice
            ) VALUES (
                @Id, @CodigoProduto, @DescricaoProduto, @TaxaJuros, @CriadoEm,
                @SimulacaoSac, @SimulacaoPrice
            )
            """;

        var parameters = new Dictionary<string, object>
        {
            { "Id", simulacao.Id },
            { "CodigoProduto", simulacao.CodigoProduto },
            { "DescricaoProduto", simulacao.DescricaoProduto },
            { "TaxaJuros", simulacao.TaxaJuros },
            { "CriadoEm", simulacao.CriadoEm },
            { "SimulacaoSac", JsonSerializer.Serialize(simulacao.Sac, AppJsonSerializerContext.Default.ResultadoSimulacao) },
            { "SimulacaoPrice", JsonSerializer.Serialize(simulacao.Price, AppJsonSerializerContext.Default.ResultadoSimulacao) }
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task RetornarTodosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = """
            SELECT Id, CodigoProduto, DescricaoProduto, TaxaJuros, CriadoEm,
                   SimulacaoSac, SimulacaoPrice
            FROM dbo.Simulacao
            """;

        var simulacoes = await connection.QueryAsync<Simulacao>(sql);
    }
}