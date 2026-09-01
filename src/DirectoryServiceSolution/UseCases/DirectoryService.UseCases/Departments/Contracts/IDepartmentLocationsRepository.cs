using DirectoryService.Core.DeparmentsContext.Entities;

namespace DirectoryService.UseCases.Departments.Contracts;

public interface IDepartmentLocationsRepository
{
    Task Add(DepartmentLocation departmentLocation, CancellationToken ct = default);

    Task DeleteByDepartmentId(Guid departmentId, CancellationToken ct = default);

    Task<IEnumerable<DepartmentLocation>> Get(
        DepartmentLocationSpecification specification,
        CancellationToken ct = default
    );
}
