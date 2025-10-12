using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;

namespace DirectoryService.Infrastructure.PostgreSQL.Seeding;

public sealed class LocationsSeeder : ISeeder
{
    private readonly ServiceDbContext _context;
    private readonly Serilog.ILogger _logger;

    public LocationsSeeder(ServiceDbContext context, Serilog.ILogger logger)
    {
        _context = context;
        _logger = logger.ForContext<LocationsSeeder>();
    }

    public async Task SeedAsync()
    {
        _logger.Information("Seeding locations...");

        try
        {
            await SeedData();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Seeding locations failed.");
        }

        _logger.Information("Locations seeded");
    }

    private async Task SeedData() { }
}