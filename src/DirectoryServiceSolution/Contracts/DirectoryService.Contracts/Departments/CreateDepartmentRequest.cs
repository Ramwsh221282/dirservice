namespace DirectoryService.Contracts.Departments;

public sealed record CreateDepartmentRequest(
    string Name,
    string Identifier,
    DepartmentLocationIdsDto LocationIds,
    Guid? ParentId = null
);
