using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed class DepartmentsIdSet
{
    private readonly List<DepartmentId> _departmentIds;
    
    public IReadOnlyList<DepartmentId> DepartmentIds => _departmentIds;
    
    private DepartmentsIdSet(IEnumerable<DepartmentId> departmentIds) =>
        _departmentIds = departmentIds.ToList();

    public static Result<DepartmentsIdSet> Create(IEnumerable<Guid> ids)
    {
        List<DepartmentId> departmentIds = [];
        foreach (Guid id in ids)
        {
            Result<DepartmentId> departmentId = DepartmentId.Create(id);
            if (departmentId.IsFailure)
                return departmentId.Error;
            departmentIds.Add(departmentId);
        }

        return Create(departmentIds);
    }
    
    public static Result<DepartmentsIdSet> Create(IEnumerable<DepartmentId> ids)
    {
        IEnumerable<DepartmentId> duplicates = ids.ExtractDuplicates(i => i.Value);
        if (duplicates.Any())
        {
            string[] duplicateIdsString = duplicates.Select(d => d.Value.ToString()).ToArray();
            string message = 
                $"""
                Найдены дубликаты идентификаторов подразделений: {string.Join(", ", duplicateIdsString)}
                """;
            return Error.ConflictError(message);
        }

        return new DepartmentsIdSet(ids);
    }
}