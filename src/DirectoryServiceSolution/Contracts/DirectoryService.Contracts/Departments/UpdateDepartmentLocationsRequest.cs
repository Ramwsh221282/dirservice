namespace DirectoryService.Contracts.Departments;

public sealed record UpdateDepartmentLocationsRequest(
    Guid DepartmentId,
    IEnumerable<Guid> LocationIds
);
