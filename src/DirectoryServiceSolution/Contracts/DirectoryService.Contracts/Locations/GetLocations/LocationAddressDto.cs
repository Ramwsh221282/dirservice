namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed class LocationAddressDto
{
    public required string FullPath { get; init; }
    public required IEnumerable<LocationAddressInfoDto> Parts { get; init; }
}

public sealed class LocationAddressInfoDto
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required int AoLevel { get; init; }
    public required string ShortName { get; init; }
}
