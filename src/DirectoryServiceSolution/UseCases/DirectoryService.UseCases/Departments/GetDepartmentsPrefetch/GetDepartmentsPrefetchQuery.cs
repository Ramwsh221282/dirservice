using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Departments.GetDepartmentsPrefetch;

public sealed record GetDepartmentsPrefetchQuery : IQuery<GetDepartmentsPrefetchResponse>
{
    public int Page { get; }
    public int PageSize { get; }
    public int Prefetch { get; }

    public GetDepartmentsPrefetchQuery(int? page = 1, int? pageSize = 20, int? prefetch = 3)
    {
        Page = page ?? 1;
        PageSize = pageSize ?? 20;
        Prefetch = prefetch ?? 3;
    }
}
