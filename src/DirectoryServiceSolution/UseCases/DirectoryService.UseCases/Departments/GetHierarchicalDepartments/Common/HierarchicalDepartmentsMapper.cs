using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;

namespace DirectoryService.UseCases.Departments.GetHierarchicalDepartments.Common;

internal sealed class HierarchicalDepartmentsMapper
{
    private readonly IEnumerable<HierarchicalDepartmentDataModel> _data;

    internal HierarchicalDepartmentsMapper(IEnumerable<HierarchicalDepartmentDataModel> data)
    {
        _data = data;
    }        

    public GetHierarchicalDepartmentsPrefetchResponse Map()
    {
        int totalCount = _data.Max(d => d.TotalCount);

        // маппинг иерархии, где узлы получают дочерние элементы.
        Dictionary<Guid, HierarchicalDepartmentDataModel> departmentsDictionary = _data.ToDictionary(d => d.Id);
        List<HierarchicalDepartmentDataModel> roots = [];

        foreach (HierarchicalDepartmentDataModel row in _data)
        {
            if (
                row.ParentId != null
                && departmentsDictionary.TryGetValue(row.ParentId.Value, out HierarchicalDepartmentDataModel? parent)
            )
            {
                parent.Childrens.Add(departmentsDictionary[row.Id]);
            }
            else
            {
                roots.Add(departmentsDictionary[row.Id]);
            }
        }

        return new GetHierarchicalDepartmentsPrefetchResponse(
            totalCount,
            roots.Select(r => r.ToResponse())
        );
    }
}
