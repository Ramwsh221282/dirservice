using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using DirectoryService.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace DirectoryService.Integrational.Tests;

public class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("database")
        .WithUsername("username")
        .WithPassword("password")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(sp =>
        {
            sp.RemoveAll<ServiceDbContext>();
            string connectionString = _dbContainer.GetConnectionString();
            IOptions<NpgSqlConnectionOptions> options = Options.Create(
                new NpgSqlConnectionOptions() { ConnectionString = connectionString }
            );
            sp.AddScoped<ServiceDbContext>(_ => new ServiceDbContext(options));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ServiceDbContext context = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
