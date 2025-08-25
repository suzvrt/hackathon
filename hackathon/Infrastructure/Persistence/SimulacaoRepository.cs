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
    public async Task<PaginacaoResultado<SimulacaoResumo>> ObterPaginadoAsync(int pagina, int qtdRegistrosPagina, string sistema)
    {
        string jsonColumnName = sistema?.ToUpperInvariant() switch
        {
            "SAC" => "SimulacaoSac",
            "PRICE" => "SimulacaoPrice",
            _ => throw new ArgumentException("Sistema de amortização inválido. Use 'SAC' ou 'PRICE'.", nameof(sistema))
        };

        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);
        var offset = (pagina - 1) * qtdRegistrosPagina;

        var countSql = "SELECT COUNT(*) FROM Simulacao;";
        var dataSql = $"""
                      SELECT Id, ValorDesejado, {jsonColumnName} AS DadosSimulacao
                      FROM Simulacao
                      ORDER BY CriadoEm DESC
                      LIMIT @QtdRegistrosPagina OFFSET @Offset;
                      """;

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
                
                var dadosSimulacao = row.DadosSimulacao;
                var valorDesejado = row.ValorDesejado ?? 0m;
                var parcelas = JsonSerializer.Deserialize(dadosSimulacao, AppJsonSerializerContext.Default.ListParcela);
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
    public string DadosSimulacao { get; set; } = "";
    public decimal? ValorDesejado { get; set; }
}

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

internal record ObterPaginadoParams(int QtdRegistrosPagina, int Offset);