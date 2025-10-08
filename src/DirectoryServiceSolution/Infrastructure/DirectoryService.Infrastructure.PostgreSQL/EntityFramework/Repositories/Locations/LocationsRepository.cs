using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Locations.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Locations;

public sealed class LocationsRepository : ILocationsRepository
{
    private readonly ServiceDbContext _dbContext;

    public LocationsRepository(ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddLocation(Location location, CancellationToken ct = default) =>
        await _dbContext.AddAsync(location, ct);

    public async Task<IEnumerable<Location>> GetBySet(
        LocationsIdSet set,
        CancellationToken ct = default
    ) =>
        await _dbContext
            .Locations.Where(loc => set.Ids.Contains(loc.Id) && loc.LifeCycle.DeletedAt == null)
            .ToListAsync(cancellationToken: ct);

    public async Task<LocationNameUniquesness> IsLocationNameUnique(
        LocationName name,
        CancellationToken ct = default
    )
    {
        bool hasAny = await _dbContext
            .Locations.AsNoTracking()
            .AnyAsync(l => l.Name == name && l.LifeCycle.DeletedAt == null, ct);

        return hasAny
            ? new LocationNameUniquesness(false, name.Value)
            : new LocationNameUniquesness(true, string.Empty);
    }
}
