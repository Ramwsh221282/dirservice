using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.Contracts;

public interface IDepartmentsRepository
{
    Task<Result<Department>> GetById(Guid id, bool useLock = false, CancellationToken ct = default);
    Task<Result<Department>> GetById(DepartmentId id, bool useLock = false, CancellationToken ct = default);

    Task DeleteSingleTimeAttachedDepartmentLocations(Department department, CancellationToken ct);
    Task DeleteSingleTimeAttachedDepartmentPositions(Department department, CancellationToken ct);

    Task ArchiveChildDepartments(Department department, DepartmentPath oldPath, CancellationToken ct);

    Task RefreshDepartmentChildPaths(
        Department department,
        DepartmentPath oldPath,
        CancellationToken ct = default
    );

    Task<IEnumerable<Department>> GetByIdArray(
        DepartmentsIdSet ids,
        CancellationToken ct = default
    );

    Task<bool> HasWithPath(DepartmentPath path, CancellationToken ct = default);

    Task Add(Department department, CancellationToken ct = default);

    Task<Result<DepartmentMovement>> GetDepartmentMovement(
        Guid parentId,
        Guid childId,
        CancellationToken ct = default
    );

    Task<DepartmentMovementApproval> GetMovementApproval(
        Department parent,
        Department child,
        CancellationToken ct = default
    );

    Task<Result<Department>> GetParentDeparmentByChildPath(
        DepartmentPath path,
        CancellationToken ct = default
    );

    Task Update(Department department, CancellationToken ct = default);

    Task<IEnumerable<Department>> Get(
        DepartmentSpecification specification,
        CancellationToken ct = default
    );
}
