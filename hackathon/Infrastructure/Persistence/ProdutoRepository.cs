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
    private readonly SqlConnectionFactory _connectionFactory;

    public ProdutoRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [DapperAot]
    public async Task<IEnumerable<Produto>> ObterProdutosCompativeisAsync(decimal valor, int prazo)
    {
        using var connection = _connectionFactory.CreateConnection();

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

        return await connection.QueryAsync<Produto>(sql, new { valor, prazo });
    }
    
    public async Task<VolumeSimuladoDiario> ObterVolumeSimuladoPorDiaAsync(DateOnly dataReferencia)
    {
        using var connection = _connectionFactory.CreateConnection();

        // 1. A query SQL busca todos os dados brutos necessários para o dia especificado.
        //    Os cálculos serão feitos na aplicação.
        var sql = """
                  SELECT 
                      CodigoProduto, DescricaoProduto, TaxaJuros, ValorDesejado, SimulacaoPrice
                  FROM dbo.Simulacao
                  WHERE CAST(CriadoEm AS DATE) = @DataReferencia
                  """;

        var parameters = new DynamicParameters();
        parameters.Add("DataReferencia", dataReferencia.ToString("yyyy-MM-dd"));

        var registrosBrutos = await connection.QueryAsync(sql, parameters);

        if (!registrosBrutos.Any())
        {
            return new VolumeSimuladoDiario
            {
                DataReferencia = dataReferencia.ToString("yyyy-MM-dd"),
                Simulacoes = new List<SimulacaoProdutoDiario>()
            };
        }

        // 2. Agrupamos os resultados por produto e calculamos as métricas.
        var simulacoesAgrupadas = registrosBrutos
            .GroupBy(row => (int)row.CodigoProduto) // Agrupa pelo código do produto
            .Select(group =>
            {
                // Para cada produto, coletamos todas as parcelas de todas as suas simulações
                var todasAsParcelas = new List<Parcela>();
                foreach (var row in group)
                {
                    var simulacaoPriceJson = (string)row.SimulacaoPrice;
                    if (!string.IsNullOrEmpty(simulacaoPriceJson))
                    {
                        var parcelasDaSimulacao = JsonSerializer.Deserialize(simulacaoPriceJson, AppJsonSerializerContext.Default.ListParcela);
                        if (parcelasDaSimulacao != null)
                        {
                            todasAsParcelas.AddRange(parcelasDaSimulacao);
                        }
                    }
                }

                // Com todas as parcelas em mãos, podemos calcular as médias e totais
                var primeiroRegistro = group.First();
                return new SimulacaoProdutoDiario
                {
                    CodigoProduto = group.Key,
                    DescricaoProduto = (string)primeiroRegistro.DescricaoProduto,

                    // Calculando as médias e somas para o grupo
                    TaxaMediaJuro = group.Average(row => (decimal?)row.TaxaJuros ?? 0m),
                    ValorTotalDesejado = group.Sum(row => (decimal?)row.ValorDesejado ?? 0m),

                    // Cálculos baseados nos dados desserializados do JSON
                    ValorMedioPrestacao = todasAsParcelas.Any() ? todasAsParcelas.Average(p => p.ValorPrestacao) : 0m,
                    ValorTotalCredito = todasAsParcelas.Any() ? todasAsParcelas.Sum(p => p.ValorPrestacao) : 0m
                };
            })
            .ToList();

        // 3. Montamos o objeto de retorno final.
        return new VolumeSimuladoDiario
        {
            DataReferencia = dataReferencia.ToString("yyyy-MM-dd"),
            Simulacoes = simulacoesAgrupadas
        };
    }
}
