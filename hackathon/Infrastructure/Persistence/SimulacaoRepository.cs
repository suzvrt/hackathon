using System.Text.Json;
using Dapper;
using hackathon.Api.Serialization;
using hackathon.Application.Dtos;
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
            { "SimulacaoSac", JsonSerializer.Serialize(simulacao.Sac, AppJsonSerializerContext.Default.ListParcela) },
            { "SimulacaoPrice", JsonSerializer.Serialize(simulacao.Price, AppJsonSerializerContext.Default.ListParcela) }
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<PaginacaoResultado<SimulacaoResumo>> ObterPaginadoAsync(int pagina, int qtdPorPagina)
    {
        using var connection = _connectionFactory.CreateConnection();

        var offset = (pagina - 1) * qtdPorPagina;

        var sql = """
            SELECT COUNT(*) FROM dbo.Simulacao;
            SELECT Id, SimulacaoPrice
            FROM dbo.Simulacao
            ORDER BY CriadoEm DESC
            OFFSET @Offset ROWS FETCH NEXT @QtdPorPagina ROWS ONLY;
        """;

        using var multi = await connection.QueryMultipleAsync(sql, new { Offset = offset, QtdPorPagina = qtdPorPagina });

        var totalRegistros = await multi.ReadSingleAsync<int>();
        var registrosBrutos = await multi.ReadAsync<(Guid Id, string SimulacaoPrice)>();

        var registros = registrosBrutos
            .Select(r =>
            {
                var parcelas = JsonSerializer.Deserialize<List<Parcela>>(r.SimulacaoPrice, AppJsonSerializerContext.Default.ListParcela);
                var primeira = parcelas?.FirstOrDefault();
                if (primeira is null)
                    return null;

                return new SimulacaoResumo
                {
                    IdSimulacao = r.Id,
                    ValorDesejado = parcelas.Sum(p => p.ValorPrestacao),
                    Prazo = parcelas.Count,
                    ValorTotalParcelas = decimal.Round(parcelas.Sum(p => p.ValorPrestacao), 2)
                };
            })
            .Where(x => x is not null)
            .ToList();

        return new PaginacaoResultado<SimulacaoResumo>
        {
            Pagina = pagina,
            QtdRegistros = totalRegistros,
            QtdRegistrosPagina = qtdPorPagina,
            Registros = registros
        };
    }
}