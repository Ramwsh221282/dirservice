using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.UseCases.Locations.Contracts;

public interface ILocationsRepository
{
    Task AddLocation(Location location, CancellationToken ct = default);
    Task<LocationNameUniquesness> IsLocationNameUnique(
        LocationName location,
        CancellationToken ct = default
    );

    Task<IEnumerable<Location>> GetBySet(LocationsIdSet Set, CancellationToken ct = default);
}
