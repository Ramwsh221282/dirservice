namespace DirectoryService.Contracts.Locations.GetLocations;

public sealed class LocationDepartmentDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string Identifier { get; init; }
    public required int ChildrensCount { get; init; }
}
