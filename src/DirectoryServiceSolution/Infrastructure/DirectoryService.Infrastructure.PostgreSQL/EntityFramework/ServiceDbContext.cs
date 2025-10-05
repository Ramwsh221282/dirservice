using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework;

public sealed class ServiceDbContext : DbContext
{
    private readonly NpgSqlConnectionOptions _options;
    private readonly ILoggerFactory _loggerFactory;

    public ServiceDbContext(IOptions<NpgSqlConnectionOptions> options, ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _loggerFactory = loggerFactory;
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Position> Positions => Set<Position>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_options.ConnectionString);
        optionsBuilder.UseLoggerFactory(_loggerFactory);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.LogTo(
            Log.Logger.Information,
            LogLevel.Information
                | LogLevel.Error
                | LogLevel.Critical
                | LogLevel.Debug
                | LogLevel.Warning
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
    }
}
