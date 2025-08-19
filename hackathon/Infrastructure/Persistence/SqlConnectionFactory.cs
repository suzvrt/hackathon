using Microsoft.Data.SqlClient;
using System.Data;
using hackathon.Infrastructure.Config;

namespace hackathon.Infrastructure.Persistence;

public class SqlConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public SqlConnectionFactory(DatabaseSettings settings)
    {
        _settings = settings;
    }

    public IDbConnection CreateConnection()
    {
        var builder = new SqlConnectionStringBuilder(_settings.ConnectionString)
        {
            ApplicationIntent = ApplicationIntent.ReadWrite,
            Encrypt = true,
            TrustServerCertificate = true
        };

        return new SqlConnection(builder.ConnectionString);
    }
}