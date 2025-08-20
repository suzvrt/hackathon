using System.Text.Json;
using Dapper;
using hackathon.Api.Serialization;
using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;

namespace hackathon.Infrastructure.Persistence;

public class SimulacaoRepository : ISimulacaoRepository
{
    private readonly HybridConnectionFactory _connectionFactory;

    public SimulacaoRepository(HybridConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task SalvarAsync(Simulacao simulacao)
    {
        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);

        var sql = """
            INSERT INTO Simulacao (
                Id, ValorDesejado, CodigoProduto, DescricaoProduto, TaxaJuros, CriadoEm,
                SimulacaoSac, SimulacaoPrice
            ) VALUES (
                @Id, @ValorDesejado, @CodigoProduto, @DescricaoProduto, @TaxaJuros, @CriadoEm,
                @SimulacaoSac, @SimulacaoPrice
            )
            """;

        var parameters = new Dictionary<string, object>
        {
            { "Id", simulacao.Id },
            { "ValorDesejado", simulacao.ValorDesejado },
            { "CodigoProduto", simulacao.CodigoProduto },
            { "DescricaoProduto", simulacao.DescricaoProduto },
            { "TaxaJuros", simulacao.TaxaJuros },
            { "CriadoEm", simulacao.CriadoEm },
            { "SimulacaoSac", JsonSerializer.Serialize(simulacao.Sac, AppJsonSerializerContext.Default.ListParcela) },
            { "SimulacaoPrice", JsonSerializer.Serialize(simulacao.Price, AppJsonSerializerContext.Default.ListParcela) }
        };

        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<PaginacaoResultado<SimulacaoResumo>> ObterPaginadoAsync(int pagina, int qtdRegistrosPagina)
    {
        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);

        var offset = (pagina - 1) * qtdRegistrosPagina;

        var countSql = "SELECT COUNT(*) FROM Simulacao;";
        var dataSql = """
                  SELECT Id, SimulacaoPrice, ValorDesejado
                  FROM Simulacao
                  ORDER BY CriadoEm DESC
                  LIMIT @QtdRegistrosPagina OFFSET @Offset;
                  """;

        var parameters = new DynamicParameters();
        parameters.Add("Offset", offset);
        parameters.Add("QtdRegistrosPagina", qtdRegistrosPagina);

        var totalRegistros = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        if (totalRegistros == 0)
        {
            return new PaginacaoResultado<SimulacaoResumo>
            {
                Pagina = pagina,
                QtdRegistros = 0,
                QtdRegistrosPagina = qtdRegistrosPagina,
                Registros = new List<SimulacaoResumo>()
            };
        }

        var registrosBrutos = await connection.QueryAsync(dataSql, parameters);

        var registros = registrosBrutos
            .Select(row =>
            {
                var idString = (string)row.Id;
                int id;
                if (Guid.TryParse(idString, out var guid))
                {
                    id = guid.GetHashCode();
                }
                else
                {
                    id = idString.GetHashCode();
                }
                var simulacaoPrice = (string)row.SimulacaoPrice;
                var valorDesejado = (decimal?)row.ValorDesejado ?? 0m;;

                var parcelas = JsonSerializer.Deserialize(simulacaoPrice, AppJsonSerializerContext.Default.ListParcela);
                if (parcelas is null || parcelas.Count == 0)
                    return null;

                return new SimulacaoResumo
                {
                    IdSimulacao = id,
                    ValorDesejado = decimal.Round(valorDesejado, 2),
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
            QtdRegistrosPagina = qtdRegistrosPagina,
            Registros = registros!
        };
    }
}