namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed record GetLocationsResponse(
    IEnumerable<LocationDto> Locations,
    int TotalCount,
    int Page,
    int PageSize
);
