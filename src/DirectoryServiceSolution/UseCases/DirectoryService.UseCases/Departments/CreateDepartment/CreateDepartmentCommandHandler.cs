using DirectoryService.Core.DeparmentsContext;
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
        if (validation.IsValid == false)
            return validation.AsFailureResult<Guid>();

        LocationsIdSet locationIds = LocationsIdSet.Create(command.LocationIds);
        IEnumerable<Location> locations = await _locationsRepository.GetBySet(locationIds, ct);
        if (locations.Any() == false)
            return Error.ConflictError(
                "Для создания подразделения необходимо указать его локацию/локации."
            );

        IDepartmentCreationStrategy strategy = command.ParentId switch
        {
            null => new RootDepartmentCreationStrategy(),
            _ => new ChildDepartmentCreationStrategy(_departmentsRepository),
        };

        Result<Department> created = await strategy.Create(command, locations, ct);
        if (created.IsFailure)
            return created.Error;
        
        await _departmentsRepository.Add(created.Value, ct);
        Result saving = await _unitOfWork.SaveChanges(ct);
        
        return saving.IsFailure ? saving.Error : created.Value.Id.Value;
    }
}
