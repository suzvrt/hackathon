namespace hackathon.Infrastructure.Config;

public class SqliteSettings
{
    public string DatabasePath { get; init; } = "hackathon.db";
    public string ConnectionString { get; init; } = "Data Source=hackathon.db;Cache=Shared;";
}
