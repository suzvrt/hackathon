using Microsoft.Data.Sqlite;
using hackathon.Domain.Entities;
using hackathon.Infrastructure.Config;
using hackathon.Application.Interfaces;

namespace hackathon.Infrastructure.Persistence;

public class TelemetriaRepository : ITelemetriaRepository
{
    private readonly SqliteSettings _settings;

    public TelemetriaRepository(SqliteSettings settings)
    {
        _settings = settings;
    }

    public async Task SalvarTelemetriaAsync(List<TelemetriaRecord> registros)
    {
        if (!registros.Any()) return;

        using var connection = new SqliteConnection(_settings.ConnectionString);
        await connection.OpenAsync();

        // Usar transação para inserção em lote
        using var transaction = await connection.BeginTransactionAsync();
        
        try
        {
            var insertCommand = """
                INSERT OR REPLACE INTO Telemetria (
                    Id, DataReferencia, NomeApi, QtdRequisicoes, TempoMedio, 
                    TempoMinimo, TempoMaximo, PercentualSucesso, CriadoEm
                ) VALUES (@Id, @DataReferencia, @NomeApi, @QtdRequisicoes, @TempoMedio, @TempoMinimo, @TempoMaximo, @PercentualSucesso, @CriadoEm)
                """;

            using var command = new SqliteCommand(insertCommand, connection, (SqliteTransaction)transaction);
            
            foreach (var registro in registros)
            {
                command.Parameters.Clear();
                command.Parameters.AddRange(new SqliteParameter[]
                {
                    new("@Id", registro.Id),
                    new("@DataReferencia", registro.DataReferencia.ToString("yyyy-MM-dd")),
                    new("@NomeApi", registro.NomeApi),
                    new("@QtdRequisicoes", registro.QtdRequisicoes),
                    new("@TempoMedio", registro.TempoMedio),
                    new("@TempoMinimo", registro.TempoMinimo),
                    new("@TempoMaximo", registro.TempoMaximo),
                    new("@PercentualSucesso", registro.PercentualSucesso),
                    new("@CriadoEm", registro.CriadoEm.ToString("yyyy-MM-dd HH:mm:ss"))
                });
                
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

    public async Task<List<TelemetriaRecord>> ObterTelemetriaPorDataAsync(DateTime dataReferencia)
    {
        using var connection = new SqliteConnection(_settings.ConnectionString);
        await connection.OpenAsync();

        var query = """
            SELECT Id, DataReferencia, NomeApi, QtdRequisicoes, TempoMedio, 
                TempoMinimo, TempoMaximo, PercentualSucesso, CriadoEm
            FROM Telemetria 
            WHERE date(DataReferencia) = date(@DataReferencia)
            ORDER BY NomeApi
            """;

        using var command = new SqliteCommand(query, connection);
        command.Parameters.Add(new SqliteParameter("@DataReferencia", dataReferencia.ToString("yyyy-MM-dd")));

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
