namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed record GetLocationsResponse(
    IEnumerable<LocationView> Locations,
    int TotalCount,
    int Page,
    int PageSize
);
