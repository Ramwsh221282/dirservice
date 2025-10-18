using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Departments.GetHierarchicalDepartments.GetDepartmentsPrefetchV2;

public sealed record GetDepartmentsPrefetchV2Query
    : IQuery<GetHierarchicalDepartmentsPrefetchResponse>
{
    public int Page { get; } = 1;
    public int PageSize { get; } = 1;
    public int Prefetch { get; } = 1;

    public GetDepartmentsPrefetchV2Query(int page, int pageSize, int prefetch)
    {
        Page = page;
        PageSize = pageSize;
        Prefetch = prefetch;
    }
}
