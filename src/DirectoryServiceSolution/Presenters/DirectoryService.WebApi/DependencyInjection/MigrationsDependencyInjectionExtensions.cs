using DirectoryService.Infrastructure.PostgreSQL.Migrations;

namespace DirectoryService.WebApi.DependencyInjection;

public static class MigrationsDependencyInjectionExtensions
{
    public static void AddMigrations(this WebApplicationBuilder builder, ApplicationConfig config)
    {
        string connectionString = string.Format(
            "Host={0};Port={1};Username={2};Password={3};Database={4}",
            config.Database.HostName,
            config.Database.Port,
            config.Database.UserName,
            config.Database.Password,
            config.Database.DatabaseName
        );

        builder.Services.AddSqlFileMigrations(connectionString);
    }
}
