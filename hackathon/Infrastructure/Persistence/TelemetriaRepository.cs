using Microsoft.Data.Sqlite;
using hackathon.Domain.Entities;
using hackathon.Application.Interfaces;
using Dapper;

namespace hackathon.Infrastructure.Persistence;

[DapperAot]
public class TelemetriaRepository : ITelemetriaRepository
{
    private readonly HybridConnectionFactory _connectionFactory;

    public TelemetriaRepository(HybridConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [DapperAot]
    public async Task SalvarTelemetriaAsync(List<TelemetriaRecord> registros)
    {
        if (registros == null || registros.Count == 0) return;

        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);
        var sqlite = (SqliteConnection)connection;

        await sqlite.OpenAsync();

        // Ajusta PRAGMAs
        using (var pragmas = sqlite.CreateCommand())
        {
            pragmas.CommandText = @"
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA busy_timeout=5000;";
            await pragmas.ExecuteNonQueryAsync();
        }

        // Agora cria a transação associada
        using var tx = sqlite.BeginTransaction();


        // Garante unicidade por (DataReferencia, NomeApi)
        using (var idx = sqlite.CreateCommand())
        {
            idx.Transaction = tx;
            idx.CommandText = @"
                CREATE UNIQUE INDEX IF NOT EXISTS UX_Telemetria_Data_Nome
                ON Telemetria (DataReferencia, NomeApi);";
            await idx.ExecuteNonQueryAsync();
        }

        const string upsertSql = @"
            INSERT INTO Telemetria (
                Id, DataReferencia, NomeApi, QtdRequisicoes, TempoMedio,
                TempoMinimo, TempoMaximo, PercentualSucesso, CriadoEm
            ) VALUES (
                @Id, @DataReferencia, @NomeApi, @QtdRequisicoes, @TempoMedio,
                @TempoMinimo, @TempoMaximo, @PercentualSucesso, @CriadoEm
            )
            ON CONFLICT(DataReferencia, NomeApi) DO UPDATE SET
                QtdRequisicoes = Telemetria.QtdRequisicoes + excluded.QtdRequisicoes,
                TempoMinimo = CASE
                    WHEN Telemetria.TempoMinimo = 0 THEN excluded.TempoMinimo
                    ELSE MIN(Telemetria.TempoMinimo, excluded.TempoMinimo)
                END,
                TempoMaximo = MAX(Telemetria.TempoMaximo, excluded.TempoMaximo),
                TempoMedio = CASE
                    WHEN (Telemetria.QtdRequisicoes + excluded.QtdRequisicoes) > 0 THEN
                        (Telemetria.TempoMedio * Telemetria.QtdRequisicoes
                        + excluded.TempoMedio * excluded.QtdRequisicoes)
                        / (Telemetria.QtdRequisicoes + excluded.QtdRequisicoes)
                    ELSE 0
                END,
                PercentualSucesso = CASE
                    WHEN (Telemetria.QtdRequisicoes + excluded.QtdRequisicoes) > 0 THEN
                        (Telemetria.PercentualSucesso * Telemetria.QtdRequisicoes
                        + excluded.PercentualSucesso * excluded.QtdRequisicoes)
                        / (Telemetria.QtdRequisicoes + excluded.QtdRequisicoes)
                    ELSE 0
                END;";

        using var cmd = sqlite.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = upsertSql;
        cmd.CommandTimeout = 5;

        // prepara parâmetros uma única vez (reutiliza no loop)
        var pId = cmd.Parameters.Add("@Id", SqliteType.Text);
        var pData = cmd.Parameters.Add("@DataReferencia", SqliteType.Text);
        var pNome = cmd.Parameters.Add("@NomeApi", SqliteType.Text);
        var pQtd = cmd.Parameters.Add("@QtdRequisicoes", SqliteType.Integer);
        var pMedio = cmd.Parameters.Add("@TempoMedio", SqliteType.Integer);
        var pMin = cmd.Parameters.Add("@TempoMinimo", SqliteType.Integer);
        var pMax = cmd.Parameters.Add("@TempoMaximo", SqliteType.Integer);
        var pPct = cmd.Parameters.Add("@PercentualSucesso", SqliteType.Real);
        var pCriadoEm = cmd.Parameters.Add("@CriadoEm", SqliteType.Text);

        foreach (var r in registros)
        {
            // Observação: o Id pode ser diferente a cada flush; no UPSERT,
            // o conflito ocorre por (DataReferencia, NomeApi) e o UPDATE preserva o Id existente.
            pId.Value = r.Id;
            pData.Value = r.DataReferencia.ToString("yyyy-MM-dd");
            pNome.Value = r.NomeApi;
            pQtd.Value = r.QtdRequisicoes;
            pMedio.Value = r.TempoMedio;
            pMin.Value = r.TempoMinimo;
            pMax.Value = r.TempoMaximo;
            pPct.Value = (double)r.PercentualSucesso;
            pCriadoEm.Value = r.CriadoEm.ToString("yyyy-MM-dd HH:mm:ss");

            await cmd.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
    }

    [DapperAot]
    public async Task<IEnumerable<TelemetriaRecord>> ObterTelemetriaPorDataAsync(DateOnly dataReferencia)
    {
        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);
        
        const string sql = @"
            SELECT
                Id, DataReferencia, NomeApi, QtdRequisicoes, TempoMedio,
                TempoMinimo, TempoMaximo, PercentualSucesso, CriadoEm
            FROM Telemetria
            WHERE DataReferencia = @DataReferenciaStr
            ORDER BY NomeApi;";
        
        var parametro = new { 
            DataReferenciaStr = dataReferencia.ToString("yyyy-MM-dd") 
        };
        
        return await connection.QueryAsync<TelemetriaRecord>(sql, parametro);
    }
}
