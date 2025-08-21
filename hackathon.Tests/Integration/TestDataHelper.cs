using Microsoft.Data.Sqlite;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;

namespace hackathon.Tests.Integration;

public static class TestDataHelper
{
    public static async Task SetupTestDatabaseAsync(SqliteConnection connection)
    {
        // Criar tabelas de teste
        await CreateTablesAsync(connection);
        
        // Inserir dados de teste
        await InsertTestDataAsync(connection);
    }

    private static async Task CreateTablesAsync(SqliteConnection connection)
    {
        var createSimulacaoTable = @"
            CREATE TABLE IF NOT EXISTS Simulacao (
                Id TEXT PRIMARY KEY,
                ValorDesejado REAL NOT NULL,
                CodigoProduto INTEGER NOT NULL,
                DescricaoProduto TEXT NOT NULL,
                TaxaJuros REAL NOT NULL,
                CriadoEm TEXT NOT NULL,
                SimulacaoSac TEXT NOT NULL,
                SimulacaoPrice TEXT NOT NULL
            );";

        var createTelemetriaTable = @"
            CREATE TABLE IF NOT EXISTS Telemetria (
                Id TEXT PRIMARY KEY,
                DataReferencia TEXT NOT NULL,
                NomeApi TEXT NOT NULL,
                QtdRequisicoes INTEGER NOT NULL,
                TempoMedio INTEGER NOT NULL,
                TempoMinimo INTEGER NOT NULL,
                TempoMaximo INTEGER NOT NULL,
                PercentualSucesso REAL NOT NULL,
                CriadoEm TEXT NOT NULL
            );";

        var createProdutoTable = @"
            CREATE TABLE IF NOT EXISTS PRODUTO (
                CO_PRODUTO INTEGER PRIMARY KEY,
                NO_PRODUTO TEXT NOT NULL,
                PC_TAXA_JUROS REAL NOT NULL,
                NU_MINIMO_MESES INTEGER NOT NULL,
                NU_MAXIMO_MESES INTEGER NOT NULL,
                VR_MINIMO REAL NOT NULL,
                VR_MAXIMO REAL NOT NULL
            );";

        await connection.ExecuteAsync(createSimulacaoTable);
        await connection.ExecuteAsync(createTelemetriaTable);
        await connection.ExecuteAsync(createProdutoTable);
    }

    private static async Task InsertTestDataAsync(SqliteConnection connection)
    {
        // Inserir produtos de teste
        var insertProdutos = @"
            INSERT OR REPLACE INTO PRODUTO (CO_PRODUTO, NO_PRODUTO, PC_TAXA_JUROS, NU_MINIMO_MESES, NU_MAXIMO_MESES, VR_MINIMO, VR_MAXIMO)
            VALUES 
                (1, 'Produto 1', 0.015, 6, 60, 1000.00, 50000.00),
                (2, 'Produto 2', 0.0175, 12, 120, 5000.00, 100000.00),
                (3, 'Produto 3', 0.02, 24, 240, 10000.00, 200000.00);";

        await connection.ExecuteAsync(insertProdutos);

        // Inserir simulações de teste
        var insertSimulacoes = @"
            INSERT OR REPLACE INTO Simulacao (Id, ValorDesejado, CodigoProduto, DescricaoProduto, TaxaJuros, CriadoEm, SimulacaoSac, SimulacaoPrice)
            VALUES 
                ('test-1', 10000.00, 1, 'Produto 1', 0.015, '2025-01-27T10:00:00Z', '{}', '{}'),
                ('test-2', 25000.00, 2, 'Produto 2', 0.0175, '2025-01-27T11:00:00Z', '{}', '{}'),
                ('test-3', 50000.00, 3, 'Produto 3', 0.02, '2025-01-27T12:00:00Z', '{}', '{}');";

        await connection.ExecuteAsync(insertSimulacoes);

        // Inserir telemetria de teste
        var insertTelemetria = @"
            INSERT OR REPLACE INTO Telemetria (Id, DataReferencia, NomeApi, QtdRequisicoes, TempoMedio, TempoMinimo, TempoMaximo, PercentualSucesso, CriadoEm)
            VALUES 
                ('telemetria-1', '2025-01-27', 'Simulacao', 150, 45, 12, 120, 0.985, '2025-01-27T23:59:59Z'),
                ('telemetria-2', '2025-01-27', 'Produtos', 75, 23, 8, 67, 0.99, '2025-01-27T23:59:59Z');";

        await connection.ExecuteAsync(insertTelemetria);
    }

    public static async Task CleanupTestDataAsync(SqliteConnection connection)
    {
        var cleanupCommands = new[]
        {
            "DELETE FROM Simulacao",
            "DELETE FROM Telemetria",
            "DELETE FROM PRODUTO"
        };

        foreach (var command in cleanupCommands)
        {
            await connection.ExecuteAsync(command);
        }
    }
}
