namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed class LocationAddressDto
{
    public required string Country { get; init; }
    public required string City { get; init; }
    public required IEnumerable<string> Additionals { get; init; }
}
