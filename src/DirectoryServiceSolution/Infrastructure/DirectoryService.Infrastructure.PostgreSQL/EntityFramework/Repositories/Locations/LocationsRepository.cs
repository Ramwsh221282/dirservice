using DirectoryService.Core.LocationsContext;
using DirectoryService.UseCases.Locations.Contracts;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Locations;

public sealed class LocationsRepository : ILocationsRepository
{
    private readonly ServiceDbContext _dbContext;

    public LocationsRepository(ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddLocation(Location location, CancellationToken ct = default)
    {
        await _dbContext.AddAsync(location, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}
