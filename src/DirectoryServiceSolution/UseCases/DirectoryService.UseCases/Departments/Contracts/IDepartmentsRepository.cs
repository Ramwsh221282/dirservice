using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.Contracts;

public interface IDepartmentsRepository
{
    Task<Result<Department>> GetById(Guid id, bool useLock = false, CancellationToken ct = default);
    Task<Result<Department>> GetById(DepartmentId id, bool useLock = false, CancellationToken ct = default);

    Task<IEnumerable<Department>> GetByIdArray(
        IEnumerable<DepartmentId> ids,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<DepartmentLocation>> GetSingleTimeAttachedDepartmentLocations(DepartmentId id, CancellationToken ct);
    Task<IReadOnlyList<DepartmentPosition>> GetSingleTimeAttachedDepartmentPositions(DepartmentId id, CancellationToken ct);
    Task<IReadOnlyList<DepartmentLocation>> GetSingleTimeAttachedDepartmentLocations(Department department, CancellationToken ct);
    Task<IReadOnlyList<DepartmentPosition>> GetSingleTimeAttachedDepartmentPositions(Department department, CancellationToken ct);

    Task RefreshDepartmentPathsFromDelete(Department department, DepartmentPath oldPath, CancellationToken ct);

    Task RefreshDepartmentChildPaths(
        Department department,
        DepartmentPath oldPath,
        CancellationToken ct = default
    );

    Task<IEnumerable<Department>> GetByIdArray(
        DepartmentsIdSet ids,
        CancellationToken ct = default
    );

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

    Task<Result<DepartmentMovement>> GetDepartmentMovement(
        DepartmentId parentId,
        DepartmentId childId,
        CancellationToken ct = default
    );

    Task<Result<Department>> GetParentDeparmentByChildPath(
        DepartmentPath path,
        CancellationToken ct = default
    );

    Task<Result<Department>> GetParentDeparmentByChildPath(
        Department department,
        CancellationToken ct = default
    );

    Task<Result<Department>> GetParentDeparmentByChildPath(
        string childPath,
        CancellationToken ct = default
    );
}
