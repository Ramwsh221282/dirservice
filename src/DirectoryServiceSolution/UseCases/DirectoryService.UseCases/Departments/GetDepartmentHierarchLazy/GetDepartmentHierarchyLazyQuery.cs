using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Departments.GetDepartmentHierarchLazy;

public sealed record GetDepartmentHierarchyLazyQuery(Guid Id)
    : IQuery<IEnumerable<LazyHierarchicalDepartmentDto>>;
