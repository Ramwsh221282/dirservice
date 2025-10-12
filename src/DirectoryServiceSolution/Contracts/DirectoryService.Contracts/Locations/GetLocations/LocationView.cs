namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed class LocationView
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string TimeZone { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime? DeletedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required LocationAddressView AddressObject { get; init; }
    public required IEnumerable<LocationDepartmentView> DepartmentObjects { get; init; }
    public required int DepartmentsCount { get; init; }
}
