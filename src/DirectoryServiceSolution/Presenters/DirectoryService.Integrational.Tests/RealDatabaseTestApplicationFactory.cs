using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.WebApi;
using DirectoryService.WebApi.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Integrational.Tests;

public sealed class RealDatabaseTestApplicationFactory
    : WebApplicationFactory<Program>,
        IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ServiceDbContext context = scope.GetService<ServiceDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }
}
