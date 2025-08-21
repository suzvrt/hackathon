using Dapper;
using hackathon.Domain.Entities;
using hackathon.Application.Interfaces;
using hackathon.Application.Dtos;
using hackathon.Domain.ValueObjects;
using System.Text.Json;
using hackathon.Api.Serialization;

namespace hackathon.Infrastructure.Persistence;

[DapperAot]
public class ProdutoRepository : IProdutoRepository
{
    private readonly HybridConnectionFactory _connectionFactory;

    public ProdutoRepository(HybridConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [DapperAot]
    public async Task<IEnumerable<Produto>> ObterProdutosCompativeisAsync(decimal valor, int prazo)
    {
        using var connection = _connectionFactory.CreateConnection(DatabaseType.SqlServer);

        var sql = """
                  SELECT CO_PRODUTO AS Codigo,
                         NO_PRODUTO AS Nome,
                         PC_TAXA_JUROS AS TaxaJuros,
                         NU_MINIMO_MESES AS MinimoMeses,
                         NU_MAXIMO_MESES AS MaximoMeses,
                         VR_MINIMO AS ValorMinimo,
                         VR_MAXIMO AS ValorMaximo
                  FROM dbo.PRODUTO
                  WHERE VR_MINIMO <= @valor AND (VR_MAXIMO IS NULL OR VR_MAXIMO >= @valor)
                  AND NU_MINIMO_MESES <= @prazo AND (NU_MAXIMO_MESES IS NULL OR NU_MAXIMO_MESES >= @prazo)
                  """;

        // MUDANÇA: Substituindo o objeto anônimo por um record nomeado.
        var parameters = new ObterProdutosParams(valor, prazo);
        return await connection.QueryAsync<Produto>(sql, parameters);
    }

    [DapperAot]
    public async Task<VolumeSimuladoDiario> ObterVolumeSimuladoPorDiaAsync(DateOnly dataReferencia, string? sistema = "PRICE")
    {
        string jsonColumnName = sistema?.ToUpperInvariant() switch
        {
            "SAC" => "SimulacaoSac",
            "PRICE" => "SimulacaoPrice",
            _ => throw new ArgumentException("Sistema de amortização inválido. Use 'SAC' ou 'PRICE'.", nameof(sistema))
        };

        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);

        var sql = $"""
                  SELECT 
                      CodigoProduto, 
                      DescricaoProduto, 
                      TaxaJuros, 
                      ValorDesejado, 
                      {jsonColumnName} AS DadosSimulacao
                  FROM Simulacao
                  WHERE date(CriadoEm) = @DataReferencia
                  """;

        // MUDANÇA: Substituindo DynamicParameters por um record nomeado.
        var parameters = new ObterVolumeParams(dataReferencia.ToString("yyyy-MM-dd"));
        
        var registrosBrutos = await connection.QueryAsync<SimulacaoDiariaDataModel>(sql, parameters);

        if (!registrosBrutos.Any())
        {
            return new VolumeSimuladoDiario
            {
                DataReferencia = dataReferencia.ToString("yyyy-MM-dd"),
                Simulacoes = []
            };
        }

        var simulacoesAgrupadas = registrosBrutos
            .GroupBy(row => row.CodigoProduto)
            .Select(group =>
            {
                var todasAsParcelas = group
                    .SelectMany(row => DeserializeParcelas(row.DadosSimulacao))
                    .ToList();

                var primeiroRegistro = group.First();
                return new SimulacaoProdutoDiario
                {
                    CodigoProduto = group.Key,
                    DescricaoProduto = primeiroRegistro.DescricaoProduto,
                    TaxaMediaJuro = group.Average(row => row.TaxaJuros ?? 0m) / 1.0000000000000000000000000000m,
                    ValorTotalDesejado = decimal.Round(group.Sum(row => row.ValorDesejado ?? 0m), 2),
                    ValorMedioPrestacao = todasAsParcelas.Any() ? decimal.Round(todasAsParcelas.Average(p => p.ValorPrestacao), 2) : 0m,
                    ValorTotalCredito = todasAsParcelas.Any() ? decimal.Round(todasAsParcelas.Sum(p => p.ValorPrestacao), 2) : 0m
                };
            })
            .ToList();

        return new VolumeSimuladoDiario
        {
            DataReferencia = dataReferencia.ToString("yyyy-MM-dd"),
            Simulacoes = simulacoesAgrupadas
        };
    }

    private IEnumerable<Parcela> DeserializeParcelas(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return Enumerable.Empty<Parcela>();
        }
        return JsonSerializer.Deserialize(json, AppJsonSerializerContext.Default.ListParcela) ?? Enumerable.Empty<Parcela>();
    }
}

// ===================================================================
// DEFINIÇÕES DE TIPOS (Acessíveis para o Source Generator)
// ===================================================================

internal class SimulacaoDiariaDataModel
{
    public int CodigoProduto { get; set; }
    public string DescricaoProduto { get; set; } = "";
    public decimal? TaxaJuros { get; set; }
    public decimal? ValorDesejado { get; set; }
    public string DadosSimulacao { get; set; } = "";
}

// Novo record para os parâmetros de ObterProdutosCompativeisAsync
internal record ObterProdutosParams(decimal valor, int prazo);

// Novo record para os parâmetros de ObterVolumeSimuladoPorDiaAsync
internal record ObterVolumeParams(string DataReferencia);