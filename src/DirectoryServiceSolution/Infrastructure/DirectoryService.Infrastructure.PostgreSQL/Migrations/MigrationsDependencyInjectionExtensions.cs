using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.PostgreSQL.Migrations;

public static class MigrationsDependencyInjectionExtensions
{
    public static IServiceCollection AddSqlFileMigrations(
        this IServiceCollection services,
        string connectionString,
        string migrationsDirectory = "migrations"
    )
    {
        services.AddSingleton(new SqlMigrationsOptions { Directory = migrationsDirectory });
        services.AddSingleton<SqlFileMigrationSource>();

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner =>
                runner.AddPostgres().WithGlobalConnectionString(connectionString)
            )
            .AddScoped<IMigrationInformationLoader, SqlFileMigrationInformationLoader>();

        return services;
    }

    public static void ApplySqlFileMigrations(this IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.ValidateVersionOrder();
        runner.MigrateUp();
    }
}
