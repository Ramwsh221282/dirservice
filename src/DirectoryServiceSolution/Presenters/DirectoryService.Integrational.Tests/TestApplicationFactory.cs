using System.Data;
using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.PostgreSQL.Database;
using DirectoryService.Infrastructure.PostgreSQL.Migrations;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using DirectoryService.UseCases.Common.Database;
using DirectoryService.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

    public TestApplicationFactory()
    {
        TestEnvironment.EnsureConfigured();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(sp =>
        {
            string connectionString = _dbContainer.GetConnectionString();

            sp.RemoveAll<NpgSqlConnectionOptions>();
            sp.RemoveAll<IDbConnectionFactory>();
            sp.RemoveAll<IIdentityConnectionFactory>();

            sp.AddSingleton(new NpgSqlConnectionOptions { ConnectionString = connectionString });
            sp.AddSingleton<IDbConnectionFactory, NpgSqlConnectionFactory>();
            sp.AddSingleton<IIdentityConnectionFactory>(
                _ => new NpgSqlIdentityConnectionFactory(connectionString)
            );

            sp.Configure<FluentMigrator.Runner.Processors.ProcessorOptions>(options =>
                options.ConnectionString = connectionString
            );
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await DatabaseMigrator.ApplyMigrations(_dbContainer.GetConnectionString());
    }

    public async Task ResetDatabase()
    {
        await DatabaseMigrator.TruncateData(_dbContainer.GetConnectionString());
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
