using System.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public static class SeederExtensions
{
    public static async Task<IServiceProvider> RunSeeders(this IServiceProvider serviceProvider)
    {
        await serviceProvider.ClearDatabase();
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IEnumerable<ISeeder> seeders = scope.ServiceProvider.GetServices<ISeeder>();

        foreach (ISeeder seeder in seeders)
        {
            await seeder.SeedAsync();
        }

        return serviceProvider;
    }

    private static async Task ClearDatabase(this IServiceProvider serviceProvider)
    {
        const string sql =
            """
            TRUNCATE TABLE
                department_positions, department_locations,
                positions, departments, locations,
                sessions, users
            RESTART IDENTITY CASCADE
            """;

        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IDbConnection connection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
        await connection.ExecuteAsync(new CommandDefinition(sql));
    }
}
