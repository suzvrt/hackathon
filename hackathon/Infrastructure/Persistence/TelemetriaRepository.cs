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
        if (!registros.Any()) return;

        // MUDANÇA 3: A conexão é criada pela factory
        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);
        await ((SqliteConnection)connection).OpenAsync();

        using var transaction = await ((SqliteConnection)connection).BeginTransactionAsync();

        try
        {
            var insertCommand = """
                INSERT OR REPLACE INTO Telemetria (
                    Id, DataReferencia, NomeApi, QtdRequisicoes, TempoMedio, 
                    TempoMinimo, TempoMaximo, PercentualSucesso, CriadoEm
                ) VALUES (@Id, @DataReferencia, @NomeApi, @QtdRequisicoes, @TempoMedio, @TempoMinimo, @TempoMaximo, @PercentualSucesso, @CriadoEm)
                """;

            using var command = new SqliteCommand(insertCommand, (SqliteConnection)connection, (SqliteTransaction)transaction);

            foreach (var registro in registros)
            {
                command.Parameters.Clear();
                command.Parameters.AddRange(
                [
                    new("@Id", registro.Id),
                    new("@DataReferencia", registro.DataReferencia.ToString("yyyy-MM-dd")),
                    new("@NomeApi", registro.NomeApi),
                    new("@QtdRequisicoes", registro.QtdRequisicoes),
                    new("@TempoMedio", registro.TempoMedio),
                    new("@TempoMinimo", registro.TempoMinimo),
                    new("@TempoMaximo", registro.TempoMaximo),
                    new("@PercentualSucesso", registro.PercentualSucesso),
                    new("@CriadoEm", registro.CriadoEm.ToString("yyyy-MM-dd HH:mm:ss"))
                ]);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [DapperAot]
    public async Task<List<TelemetriaRecord>> ObterTelemetriaPorDataAsync(DateTime dataReferencia)
    {
        using var connection = _connectionFactory.CreateConnection(DatabaseType.Sqlite);

        var query = """
            SELECT Id, DataReferencia, NomeApi, QtdRequisicoes, TempoMedio, 
                   TempoMinimo, TempoMaximo, PercentualSucesso, CriadoEm
            FROM Telemetria 
            WHERE date(DataReferencia) = date(@DataReferencia)
            ORDER BY NomeApi
            """;

        using var command = new SqliteCommand(query, (SqliteConnection)connection);
        command.Parameters.Add(new SqliteParameter("@DataReferencia", dataReferencia.ToString("yyyy-MM-dd")));
        await ((SqliteConnection)connection).OpenAsync();

        var registros = new List<TelemetriaRecord>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            registros.Add(new TelemetriaRecord
            {
                Id = reader.GetString(0),
                DataReferencia = DateTime.Parse(reader.GetString(1)),
                NomeApi = reader.GetString(2),
                QtdRequisicoes = reader.GetInt32(3),
                TempoMedio = reader.GetInt32(4),
                TempoMinimo = reader.GetInt32(5),
                TempoMaximo = reader.GetInt32(6),
                PercentualSucesso = reader.GetDecimal(7),
                CriadoEm = DateTime.Parse(reader.GetString(8))
            });
        }

        return registros;
    }
}