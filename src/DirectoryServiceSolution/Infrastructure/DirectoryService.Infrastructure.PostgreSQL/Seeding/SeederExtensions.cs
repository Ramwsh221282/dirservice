using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public static class SeederExtensions
{
    public static async Task<IServiceProvider> RunSeeders(this IServiceProvider serviceProvider)
    {
        await serviceProvider.RecreateDatabase();
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IEnumerable<ISeeder> seeders = scope.ServiceProvider.GetServices<ISeeder>();

        foreach (ISeeder seeder in seeders)
            await seeder.SeedAsync();

        return serviceProvider;
    }

    private static async Task RecreateDatabase(this IServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        ServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
