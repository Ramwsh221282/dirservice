using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Locations.Contracts;
using Microsoft.EntityFrameworkCore;
using ResultLibrary;

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
            .Locations
            .Include(l => l.Departments)
            .Where(loc => set.Ids.Contains(loc.Id) && loc.LifeCycle.DeletedAt == null)
            .ToListAsync(cancellationToken: ct);

    public async Task<Result<Location>> GetById(Guid id, CancellationToken ct = default)
    {
        LocationId locationId = new LocationId(id);
        return await GetById(locationId, ct);
    }

    public async Task<Result<Location>> GetById(LocationId id, CancellationToken ct = default)
    {
        Location? location = await _dbContext.Locations
            .Include(l => l.Departments)
            .FirstOrDefaultAsync(cancellationToken: ct);
        return location == null ? Error.NotFoundError("Локация не найдена") : location;
    }

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