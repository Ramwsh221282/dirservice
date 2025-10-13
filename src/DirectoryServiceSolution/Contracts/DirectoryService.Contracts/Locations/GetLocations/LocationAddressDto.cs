namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed class LocationAddressDto
{
    public required IEnumerable<LocationAddressPartDto> Nodes { get; init; }
}
