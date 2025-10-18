using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;

namespace DirectoryService.UseCases.Departments.GetHierarchicalDepartments.Common;

internal sealed class GetDepartmentsPrefetchDataModel
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
    public List<GetDepartmentsPrefetchDataModel> Childrens { get; } = [];
    public required bool HasMoreChildren { get; init; }
    public required int TotalCount { get; init; }

    public HierarchicalDepartmentDto ToResponse()
    {
        var dto = new HierarchicalDepartmentDto()
        {
            Id = Id,
            Identifier = Identifier,
            Name = Name,
            Path = Path,
            Depth = Depth,
            ParentId = ParentId,
            ChildrensCount = ChildrensCount,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            HasMoreChildren = HasMoreChildren,
        };

        dto.Childrens.AddRange(Childrens.Select(c => c.ToResponse()));
        return dto;
    }
}
