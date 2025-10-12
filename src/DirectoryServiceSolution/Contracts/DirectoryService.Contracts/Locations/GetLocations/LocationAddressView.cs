namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed class LocationAddressView
{
    public required IEnumerable<LocationAddressPartView> Parts { get; init; }
}
