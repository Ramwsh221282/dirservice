namespace DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;

public sealed record GetDepartmentsPrefetchResponse(
    int TotalCount,
    IEnumerable<GetDepartmentsPrefetchDto> Hierarchy
);
