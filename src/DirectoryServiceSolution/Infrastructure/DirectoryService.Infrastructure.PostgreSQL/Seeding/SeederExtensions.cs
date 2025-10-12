using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public static class SeederExtensions
{
    public static async Task<IServiceProvider> RunSeeders(this IServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IEnumerable<ISeeder> seeders = scope.ServiceProvider.GetServices<ISeeder>();

        foreach (ISeeder seeder in seeders)
            await seeder.SeedAsync();

        return serviceProvider;
    }
}