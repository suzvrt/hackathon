using Microsoft.Data.Sqlite;
using hackathon.Infrastructure.Config;

namespace hackathon.Infrastructure.Persistence;

public interface ISqliteInitializer
{
    Task InitializeAsync();
}

public class SqliteInitializer : ISqliteInitializer
{
    private readonly SqliteSettings _settings;

    public SqliteInitializer(SqliteSettings settings)
    {
        _settings = settings;
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_settings.ConnectionString);
        await connection.OpenAsync();

        // Criar tabela Simulacao
        var createSimulacaoTable = """
            CREATE TABLE IF NOT EXISTS Simulacao (
                Id TEXT PRIMARY KEY,
                ValorDesejado NUMERIC(10,2) NOT NULL,
                CodigoProduto INTEGER NOT NULL,
                DescricaoProduto TEXT NOT NULL,
                TaxaJuros NUMERIC(10,9) NOT NULL,
                CriadoEm TEXT NOT NULL,
                SimulacaoSac TEXT NOT NULL,
                SimulacaoPrice TEXT NOT NULL
            );
            """;

        using (var command = new SqliteCommand(createSimulacaoTable, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // Criar índices para melhor performance
        var createIndexes = """
            CREATE INDEX IF NOT EXISTS IX_Simulacao_CriadoEm ON Simulacao(CriadoEm);
            CREATE INDEX IF NOT EXISTS IX_Simulacao_CodigoProduto ON Simulacao(CodigoProduto);
            """;

        using (var command = new SqliteCommand(createIndexes, connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }
}
