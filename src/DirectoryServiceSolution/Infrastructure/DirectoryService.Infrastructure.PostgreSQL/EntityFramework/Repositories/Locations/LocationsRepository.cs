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

    public async Task<Result<Guid>> AddLocation(Location location, CancellationToken ct = default)
    {
        try
        {
            await _dbContext.AddAsync(location, ct);
            await _dbContext.SaveChangesAsync(ct);
            return location.Id.Value;
        }
        catch
        {
            return Error.ExceptionalError("Непредвиденная ошибка при сохранении локации.");
        }
    }

    public async Task<LocationNameUniquesness> IsLocationNameUnique(
        LocationName name,
        CancellationToken ct = default
    )
    {
        bool hasAny = await _dbContext.Locations.AsNoTracking().AnyAsync(l => l.Name == name, ct);
        return hasAny
            ? new LocationNameUniquesness(false, name.Value)
            : new LocationNameUniquesness(true, string.Empty);
    }
}
