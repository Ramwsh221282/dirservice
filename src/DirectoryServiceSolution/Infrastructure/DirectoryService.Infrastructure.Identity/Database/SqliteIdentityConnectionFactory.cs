using System.Data;
using DirectoryService.Infrastructure.Identity.Options;
using Microsoft.Data.Sqlite;

namespace DirectoryService.Infrastructure.Identity.Database;

public sealed class SqliteIdentityConnectionFactory : IIdentityConnectionFactory
{
    private readonly string _connectionString;

    public SqliteIdentityConnectionFactory(IdentityConnectionOptions options)
    {
        _connectionString = options.ConnectionString;
    }

    public IDbConnection Create()
    {
        SqliteConnection connection = new(_connectionString);
        connection.Open();
        ApplyPragmas(connection);
        return connection;
    }

    private static void ApplyPragmas(SqliteConnection connection)
    {
        using IDbCommand command = connection.CreateCommand();
        command.CommandText =
            """
            PRAGMA foreign_keys = ON;
            PRAGMA synchronous = NORMAL;
            """;
        command.ExecuteNonQuery();
    }
}
