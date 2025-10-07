using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.UseCases.Departments.Contracts;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.CreateDepartment;

internal sealed class ChildDepartmentCreationStrategy : IDepartmentCreationStrategy
{
    private readonly IDepartmentsRepository _repository;

    public ChildDepartmentCreationStrategy(IDepartmentsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Department>> Create(
        CreateDepartmentCommand command,
        IEnumerable<Location> locations,
        CancellationToken ct = default
    )
    {
        if (command.ParentId == null)
            return Error.ValidationError(
                "Невозможно создать дочернее подразделение. Не указан ID основного подразделения."
            );

        Result<Department> parent = await _repository.GetById(command.ParentId.Value, ct);
        if (parent.IsFailure)
            return parent.Error;

        DepartmentName name = DepartmentName.Create(command.Name);
        DepartmentIdentifier identifier = DepartmentIdentifier.Create(command.Identifier);
        Department department = Department.CreateNew(name, identifier);
        Result addingLocations = department.AddLocations(locations);
        if (addingLocations.IsFailure)
            return addingLocations.Error;

        parent.Value.AttachOtherDepartment(department);
        return department;
    }
}
