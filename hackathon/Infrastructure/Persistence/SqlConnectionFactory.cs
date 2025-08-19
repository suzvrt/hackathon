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

    public IDbConnection CreateConnection() => new SqlConnection(_settings.ConnectionString);
}
