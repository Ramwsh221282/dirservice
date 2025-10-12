namespace DirectoryService.Contracts.Locations;

public sealed class LocationAddressView
{
    public required IEnumerable<LocationAddressPartView> Parts { get; init; }
}
