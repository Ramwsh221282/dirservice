using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;

namespace DirectoryService.UseCases.Departments.GetHierarchicalDepartments.Common;

internal sealed class HierarchicalDepartmentsMapper
{
    private readonly IEnumerable<HierarchicalDepartmentDataModel> _data;

    internal HierarchicalDepartmentsMapper(IEnumerable<HierarchicalDepartmentDataModel> data) =>
        _data = data;

    public GetHierarchicalDepartmentsPrefetchResponse Map()
    {
        var totalCount = _data.Select(d => d.TotalCount).Max();

        // маппинг иерархии, где узлы получают дочерние элементы.
        var departmentsDictionary = _data.ToDictionary(d => d.Id);
        var roots = new List<HierarchicalDepartmentDataModel>();

        foreach (var row in _data)
        {
            if (
                row.ParentId != null
                && departmentsDictionary.TryGetValue(row.ParentId.Value, out var parent)
            )
                parent.Childrens.Add(departmentsDictionary[row.Id]);
            else
                roots.Add(departmentsDictionary[row.Id]);
        }

        return new GetHierarchicalDepartmentsPrefetchResponse(
            totalCount,
            roots.Select(r => r.ToResponse())
        );
    }
}
