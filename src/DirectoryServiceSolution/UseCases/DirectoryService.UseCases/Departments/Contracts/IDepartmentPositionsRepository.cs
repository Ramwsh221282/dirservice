using DirectoryService.Core.DeparmentsContext.Entities;

namespace DirectoryService.UseCases.Departments.Contracts;

public interface IDepartmentPositionsRepository
{
    Task Add(DepartmentPosition departmentPosition, CancellationToken ct = default);

    Task<IEnumerable<DepartmentPosition>> Get(
        DepartmentPositionSpecification specification,
        CancellationToken ct = default
    );
}
