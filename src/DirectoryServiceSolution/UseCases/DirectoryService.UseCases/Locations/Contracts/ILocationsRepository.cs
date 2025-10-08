using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;

namespace DirectoryService.UseCases.Locations.Contracts;

public interface ILocationsRepository
{
    Task AddLocation(Location location, CancellationToken ct = default);
    Task<LocationNameUniquesness> IsLocationNameUnique(
        LocationName name,
        CancellationToken ct = default
    );

    Task<IEnumerable<Location>> GetBySet(LocationsIdSet set, CancellationToken ct = default);
}
