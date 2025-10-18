namespace DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;

public sealed record GetHierarchicalDepartmentsPrefetchResponse(
    int TotalCount,
    IEnumerable<HierarchicalDepartmentDto> Hierarchy
);
