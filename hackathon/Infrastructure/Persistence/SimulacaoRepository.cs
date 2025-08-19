using System.Text.Json;
using Dapper;
using hackathon.Api.Serialization;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;

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

        await connection.ExecuteAsync(sql, new
        {
            simulacao.Id,
            simulacao.CodigoProduto,
            simulacao.DescricaoProduto,
            TaxaJuros = JsonSerializer.Serialize(simulacao.TaxaJuros, AppJsonSerializerContext.Default.Decimal),
            CriadoEm = JsonSerializer.Serialize(simulacao.CriadoEm, AppJsonSerializerContext.Default.DateTime),
            SimulacaoSac = JsonSerializer.Serialize(simulacao.Sac, AppJsonSerializerContext.Default.ResultadoSimulacao),
            SimulacaoPrice = JsonSerializer.Serialize(simulacao.Price, AppJsonSerializerContext.Default.ResultadoSimulacao)
        });
    }
}