using DirectoryService.Core.LocationsContext;
using ResultLibrary;

namespace DirectoryService.UseCases.Locations.Contracts;

public interface ILocationsRepository
{
    Task<Result<Guid>> AddLocation(Location location, CancellationToken ct = default);
}
