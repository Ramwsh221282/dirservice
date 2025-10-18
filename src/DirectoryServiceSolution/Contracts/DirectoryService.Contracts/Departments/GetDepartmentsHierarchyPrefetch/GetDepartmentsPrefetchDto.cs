namespace DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;

public sealed class GetDepartmentsPrefetchDto
{
    public required Guid Id { get; init; }
    public required string Identifier { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required int Depth { get; init; }
    public required Guid? ParentId { get; init; }
    public required int ChildrensCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public List<GetDepartmentsPrefetchDto> Childrens { get; } = [];
    public required bool HasMoreChildren { get; init; }
    public required int TotalCount { get; init; }
}
