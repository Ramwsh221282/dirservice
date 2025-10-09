using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.UseCases.Locations.Contracts;

public interface ILocationsRepository
{
    Task AddLocation(Location location, CancellationToken ct = default);

    Task<LocationNameUniquesness> IsLocationNameUnique(
        LocationName name,
        CancellationToken ct = default
    );

    Task<IEnumerable<Location>> GetBySet(LocationsIdSet set, CancellationToken ct = default);

    Task<Result<Location>> GetById(Guid id, CancellationToken ct = default);
    Task<Result<Location>> GetById(LocationId id, CancellationToken ct = default);
}