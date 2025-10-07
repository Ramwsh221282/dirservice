using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.CreateDepartment;

internal sealed class RootDepartmentCreationStrategy : IDepartmentCreationStrategy
{
    public Task<Result<Department>> Create(
        CreateDepartmentCommand command,
        IEnumerable<Location> locations,
        CancellationToken ct = default
    )
    {
        DepartmentName name = DepartmentName.Create(command.Name);
        DepartmentIdentifier identifier = DepartmentIdentifier.Create(command.Identifier);
        Department department = Department.CreateNew(name, identifier);
        Result addingLocations = department.AddLocations(locations);
        if (addingLocations.IsFailure)
        {
            Result<Department> failed = Result<Department>.Fail(addingLocations.Error);
            return Task.FromResult(failed);
        }

        return Task.FromResult(Result<Department>.Success(department));
    }
}
