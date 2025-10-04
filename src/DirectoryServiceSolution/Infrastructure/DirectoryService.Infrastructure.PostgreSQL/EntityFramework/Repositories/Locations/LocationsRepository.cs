using DirectoryService.Core.LocationsContext;
using DirectoryService.UseCases.Locations.Contracts;
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
}
