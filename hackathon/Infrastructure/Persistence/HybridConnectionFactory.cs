using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System.Data;
using hackathon.Infrastructure.Config;

namespace hackathon.Infrastructure.Persistence;

public enum DatabaseType
{
    SqlServer,
    Sqlite
}

public interface IHybridConnectionFactory
{
    IDbConnection CreateConnection(DatabaseType databaseType = DatabaseType.Sqlite);
}

public class HybridConnectionFactory : IHybridConnectionFactory
{
    private readonly DatabaseSettings _sqlServerSettings;
    private readonly SqliteSettings _sqliteSettings;

    public HybridConnectionFactory(DatabaseSettings sqlServerSettings, SqliteSettings sqliteSettings)
    {
        _sqlServerSettings = sqlServerSettings;
        _sqliteSettings = sqliteSettings;
    }

    public IDbConnection CreateConnection(DatabaseType databaseType = DatabaseType.Sqlite)
    {
        return databaseType switch
        {
            DatabaseType.SqlServer => CreateSqlServerConnection(),
            DatabaseType.Sqlite => CreateSqliteConnection(),
            _ => throw new ArgumentException($"Tipo de banco de dados não suportado: {databaseType}")
        };
    }

    private IDbConnection CreateSqlServerConnection()
    {
        var builder = new SqlConnectionStringBuilder(_sqlServerSettings.ConnectionString)
        {
            ApplicationIntent = ApplicationIntent.ReadWrite,
            Encrypt = true,
            TrustServerCertificate = true
        };

        return new SqlConnection(builder.ConnectionString);
    }

    private IDbConnection CreateSqliteConnection()
    {
        return new SqliteConnection(_sqliteSettings.ConnectionString);
    }
}
