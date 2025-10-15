namespace DirectoryService.Contracts.Departments.UpdateDepartment;

public sealed record UpdateDepartmentLocationsRequest(
    Guid DepartmentId,
    IEnumerable<Guid> LocationIds
);
