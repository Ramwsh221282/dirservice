using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Common.UnitOfWork;
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
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(
        IValidator<CreateDepartmentCommand> validator,
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IUnitOfWork unitOfWork
    )
    {
        _validator = validator;
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateDepartmentCommand command,
        CancellationToken ct = default
    )
    {
        ValidationResult validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return validation.AsFailureResult<Guid>();

        LocationsIdSet locationIds = LocationsIdSet.Create(command.LocationIds);
        IEnumerable<Location> locations = await _locationsRepository.GetBySet(locationIds, ct);
        if (!locations.Any())
            return Error.ConflictError(
                "Для создания подразделения необходимо указать его локацию/локации."
            );

        DepartmentName name = DepartmentName.Create(command.Name);
        DepartmentIdentifier identifier = DepartmentIdentifier.Create(command.Identifier);

        Department? parent = null;
        if (command.ParentId != null)
        {
            Result<Department> parentResult = await _departmentsRepository.GetById(
                command.ParentId.Value,
                ct
            );
            if (parentResult.IsFailure)
                return parentResult.Error;

            parent = parentResult.Value;
        }

        Result<Department> department = Department.CreateNew(name, identifier, locations, parent);
        await _departmentsRepository.Add(department.Value, ct);
        Result saving = await _unitOfWork.SaveChanges(ct);

        return saving.IsFailure ? saving.Error : department.Value.Id.Value;
    }
}
