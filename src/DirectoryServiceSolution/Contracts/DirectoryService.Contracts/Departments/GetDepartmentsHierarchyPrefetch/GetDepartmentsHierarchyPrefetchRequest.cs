namespace DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;

public sealed record GetDepartmentsHierarchyPrefetchRequest
{
    public int Page { get; }
    public int PageSize { get; }
    public int Prefetch { get; }

    public GetDepartmentsHierarchyPrefetchRequest(int? page, int? pageSize, int? prefetch)
    {
        Page = page ?? 1;
        PageSize = pageSize ?? 20;
        Prefetch = prefetch ?? 3;
    }
}
