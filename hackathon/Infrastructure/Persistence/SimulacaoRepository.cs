using System.Text.Json;
using Dapper;
using hackathon.Api.Serialization;
using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;

namespace hackathon.Infrastructure.Persistence;

[DapperAot]
public class SimulacaoRepository : ISimulacaoRepository
{
    private readonly HybridConnectionFactory _connectionFactory;

    public SimulacaoRepository(HybridConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [DapperAot]
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
        
        // MUDANÇA: Usando um 'record' nomeado em vez de um objeto anônimo
        var parameters = new SalvarSimulacaoParams(
            simulacao.Id,
            simulacao.ValorDesejado,
            simulacao.CodigoProduto,
            simulacao.DescricaoProduto,
            simulacao.TaxaJuros,
            simulacao.CriadoEm,
            JsonSerializer.Serialize(simulacao.Sac, AppJsonSerializerContext.Default.ListParcela),
            JsonSerializer.Serialize(simulacao.Price, AppJsonSerializerContext.Default.ListParcela)
        );

        await connection.ExecuteAsync(sql, parameters);
    }

    [DapperAot]
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

        // MUDANÇA: Usando um 'record' nomeado
        var parameters = new ObterPaginadoParams(qtdRegistrosPagina, offset);

        var totalRegistros = await connection.ExecuteScalarAsync<int>(countSql);
        if (totalRegistros == 0)
        {
            return new PaginacaoResultado<SimulacaoResumo>
            {
                Pagina = pagina, QtdRegistros = 0, QtdRegistrosPagina = qtdRegistrosPagina, Registros = []
            };
        }

        var registrosBrutos = await connection.QueryAsync<SimulacaoDataModel>(dataSql, parameters);
        
        var registros = registrosBrutos
            .Select(row =>
            {
                var idString = row.Id;
                int id;
                if (Guid.TryParse(idString, out var guid)) { id = guid.GetHashCode(); }
                else { id = idString.GetHashCode(); }
                
                var simulacaoPrice = row.SimulacaoPrice;
                var valorDesejado = row.ValorDesejado ?? 0m;
                var parcelas = JsonSerializer.Deserialize(simulacaoPrice, AppJsonSerializerContext.Default.ListParcela);
                if (parcelas is null || parcelas.Count == 0) return null;

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
            Pagina = pagina, QtdRegistros = totalRegistros, QtdRegistrosPagina = qtdRegistrosPagina, Registros = registros!
        };
    }
}

// ===================================================================
// DEFINIÇÕES DE TIPOS (Acessíveis para o Source Generator)
// ===================================================================

internal class SimulacaoDataModel
{
    public string Id { get; set; } = "";
    public string SimulacaoPrice { get; set; } = "";
    public decimal? ValorDesejado { get; set; }
}

// Novo record para os parâmetros de salvar
internal record SalvarSimulacaoParams(
    Guid Id,
    decimal ValorDesejado,
    int CodigoProduto,
    string DescricaoProduto,
    decimal TaxaJuros,
    DateTime CriadoEm,
    string SimulacaoSac,
    string SimulacaoPrice
);

// Novo record para os parâmetros de paginação
internal record ObterPaginadoParams(int QtdRegistrosPagina, int Offset);