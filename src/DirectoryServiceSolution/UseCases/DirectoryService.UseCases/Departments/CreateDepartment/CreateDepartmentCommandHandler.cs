using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Common.Transaction;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Locations.Contracts;
using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;

namespace DirectoryService.UseCases.Departments.CreateDepartment;

public sealed class CreateDepartmentCommandHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IDepartmentLocationsRepository _departmentLocationsRepository;
    private readonly ITransactionSource _transactionSource;

    public CreateDepartmentCommandHandler(
        IValidator<CreateDepartmentCommand> validator,
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IDepartmentLocationsRepository departmentLocationsRepository,
        ITransactionSource transactionSource
    )
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _departmentLocationsRepository = departmentLocationsRepository;
        _transactionSource = transactionSource;
    }

    public async Task<Result<Guid>> Handle(
        CreateDepartmentCommand command,
        CancellationToken ct = default
    )
    {
        ValidationResult validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return validation.AsFailureResult<Guid>();
        }

        await using ITransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);

        LocationsIdSet locationIds = LocationsIdSet.Create(command.LocationIds);
        IEnumerable<Location> locations = await _locationsRepository.GetBySet(locationIds, ct);
        if (!locations.Any())
        {
            string message = "Для создания подразделения необходимо указать его локацию/локации.";
            return Error.ConflictError(message);
        }

        DepartmentName name = DepartmentName.Create(command.Name);
        DepartmentIdentifier identifier = DepartmentIdentifier.Create(command.Identifier);

        Department? parent = null;
        if (command.ParentId != null)
        {
            Result<Department> parentResult = await _departmentsRepository.GetById(
                command.ParentId.Value, ct: ct
            );
            if (parentResult.IsFailure)
            {
                return parentResult.Error;
            }

            parent = parentResult.Value;
        }

        DepartmentPath path;
        if (parent == null)
        {
            path = new DepartmentPath(identifier);
        }
        else
        {
            Result<DepartmentPath> childPath = parent.Path.BindWithOther(identifier);
            if (childPath.IsFailure)
            {
                return childPath.Error;
            }

            path = childPath.Value;
        }

        if (await _departmentsRepository.HasWithPath(path, ct))
        {
            return Error.ConflictError(
                $"Подразделение с идентификатором '{identifier.Value}' уже существует по пути '{path.Value}'."
            );
        }

        Result<Department> department = Department.CreateNew(name, identifier, locations, parent);
        if (department.IsFailure)
        {
            return department.Error;
        }

        await _departmentsRepository.Add(department.Value, ct);

        foreach (DepartmentLocation departmentLocation in department.Value.Locations)
        {
            await _departmentLocationsRepository.Add(departmentLocation, ct);
        }

        if (parent != null)
        {
            await _departmentsRepository.Update(parent, ct);
        }

        Result committing = await transaction.CommitChanges(nameof(CreateDepartmentCommand), ct);
        if (committing.IsFailure)
        {
            return committing.Error;
        }

        return department.Value.Id.Value;
    }
}
