namespace DirectoryService.Contracts.Locations;

public sealed class GetLocationsResponse
{
    public required IEnumerable<LocationView> Locations { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
