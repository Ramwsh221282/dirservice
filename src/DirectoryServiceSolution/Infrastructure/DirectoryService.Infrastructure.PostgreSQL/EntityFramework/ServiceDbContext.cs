using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework;

public sealed class ServiceDbContext : DbContext
{
    private readonly NpgSqlConnectionOptions _options;

    public ServiceDbContext(IOptions<NpgSqlConnectionOptions> options)
    {
        _options = options.Value;
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Position> Positions => Set<Position>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_options.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
    }
}
