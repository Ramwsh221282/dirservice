namespace DirectoryService.Contracts.Departments.CreateDepartment;

public sealed record CreateDepartmentRequest(
    string Name,
    string Identifier,
    IEnumerable<Guid> LocationIds,
    Guid? ParentId = null
);
