using Dapper;
using hackathon.Domain.Entities;
using hackathon.Application.Interfaces;

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
}
