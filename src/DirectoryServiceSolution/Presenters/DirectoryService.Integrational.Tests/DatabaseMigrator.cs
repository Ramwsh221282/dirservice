using DirectoryService.Infrastructure.PostgreSQL.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DirectoryService.Integrational.Tests;

public static class DatabaseMigrator
{
    public static Task ApplyMigrations(string connectionString)
    {
        ServiceCollection services = new();
        services.AddSqlFileMigrations(connectionString);

        using ServiceProvider provider = services.BuildServiceProvider(false);
        provider.ApplySqlFileMigrations();

        return Task.CompletedTask;
    }

    public static async Task TruncateData(string connectionString)
    {
        const string sql =
            """
            TRUNCATE TABLE
                department_positions, department_locations,
                positions, departments, locations,
                sessions, users
            RESTART IDENTITY CASCADE
            """;

        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
