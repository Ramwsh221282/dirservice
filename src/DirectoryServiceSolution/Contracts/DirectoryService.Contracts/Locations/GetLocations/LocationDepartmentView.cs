namespace DirectoryService.Contracts.Locations;

public sealed class LocationDepartmentView
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string Identifier { get; init; }
    public required int ChildrensCount { get; init; }
}
