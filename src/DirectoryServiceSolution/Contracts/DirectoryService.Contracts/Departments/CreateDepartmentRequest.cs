namespace DirectoryService.Contracts.Departments;

public sealed record CreateDepartmentRequest(
    DepartmentNameDto Name,
    DepartmentIdentifierDto Identifier,
    IEnumerable<DepartmentLocationIdDto> Locations,
    DepartmentIdDto? ParentId = null
);
