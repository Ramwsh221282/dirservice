namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed class LocationDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string TimeZone { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime? DeletedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required LocationAddressDto Address { get; init; }
    public required IEnumerable<LocationDepartmentDto> Departments { get; init; }
    public required int DepartmentsCount { get; init; }
}
