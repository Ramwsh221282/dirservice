using DirectoryService.Core.LocationsContext;

namespace DirectoryService.UseCases.Locations.Contracts;

public interface ILocationsRepository
{
    Task AddLocation(Location location, CancellationToken ct = default);
}
