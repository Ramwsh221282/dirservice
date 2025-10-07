using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.LocationsContext;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.CreateDepartment;

internal interface IDepartmentCreationStrategy
{
    Task<Result<Department>> Create(
        CreateDepartmentCommand command,
        IEnumerable<Location> locations,
        CancellationToken ct = default
    );
}
