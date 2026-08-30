using System.Data;

namespace DirectoryService.Infrastructure.Identity.Database;

public sealed class IdentitySchemaInitializer
{
    private const string CreateUsersTableSql =
        """
        CREATE TABLE IF NOT EXISTS users (
            id TEXT NOT NULL PRIMARY KEY,
            login TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL,
            created_at TEXT NOT NULL,
            updated_at TEXT NOT NULL,
            deleted_at TEXT NULL
        );
        """;

    private const string CreateSessionsTableSql =
        """
        CREATE TABLE IF NOT EXISTS sessions (
            session_id TEXT NOT NULL PRIMARY KEY,
            user_id TEXT NOT NULL,
            refresh_token TEXT NULL,
            access_token TEXT NULL,
            created_at TEXT NOT NULL,
            access_token_expires_at TEXT NOT NULL,
            refresh_token_expires_at TEXT NOT NULL,
            FOREIGN KEY (user_id) REFERENCES users (id)
        );
        """;

    private readonly IIdentityConnectionFactory _connectionFactory;

    public IdentitySchemaInitializer(IIdentityConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void EnsureCreated()
    {
        using IDbConnection connection = _connectionFactory.Create();

        using (IDbCommand journalModeCommand = connection.CreateCommand())
        {
            journalModeCommand.CommandText = "PRAGMA journal_mode = WAL;";
            journalModeCommand.ExecuteNonQuery();
        }

        using (IDbCommand usersCommand = connection.CreateCommand())
        {
            usersCommand.CommandText = CreateUsersTableSql;
            usersCommand.ExecuteNonQuery();
        }

        using (IDbCommand sessionsCommand = connection.CreateCommand())
        {
            sessionsCommand.CommandText = CreateSessionsTableSql;
            sessionsCommand.ExecuteNonQuery();
        }
    }
}
