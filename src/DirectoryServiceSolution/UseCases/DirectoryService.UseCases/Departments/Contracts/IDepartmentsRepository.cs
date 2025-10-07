using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.Contracts;

public interface IDepartmentsRepository
{
    Task<Result<Department>> GetById(Guid id, CancellationToken ct = default);
    Task<Result<Department>> GetById(DepartmentId id, CancellationToken ct = default);
    Task<IEnumerable<Department>> GetByIdArray(IEnumerable<DepartmentId> ids, CancellationToken ct = default);
    Task<IEnumerable<Department>> GetByIdArray(DepartmentsIdSet ids, CancellationToken ct = default);
    Task Add(Department department, CancellationToken ct = default);
}
