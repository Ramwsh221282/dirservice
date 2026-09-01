using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
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
using Serilog;

namespace DirectoryService.UseCases.Departments.UpdateDepartmentLocations;

public sealed class UpdateDepartmentLocationsCommandHandler
    : ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
{
    private readonly ITransactionSource _transactionSource;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentLocationsRepository _departmentLocationsRepository;
    private readonly ILogger _logger;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;

    public UpdateDepartmentLocationsCommandHandler(
        ITransactionSource transactionSource,
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IDepartmentLocationsRepository departmentLocationsRepository,
        ILogger logger,
        IValidator<UpdateDepartmentLocationsCommand> validator
    )
    {
        _transactionSource = transactionSource;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _departmentLocationsRepository = departmentLocationsRepository;
        _logger = logger.BindTo<UpdateDepartmentLocationsCommand>();
        _validator = validator;
    }

    public async Task<Result<Guid>> Handle(
        UpdateDepartmentLocationsCommand command,
        CancellationToken ct = default
    )
    {
        ValidationResult validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            Result<Guid> fail = validation.AsFailureResult<Guid>();
            return _logger.ReturnLogged(fail);
        }

        await using ITransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);
        LocationsIdSet locationIds = LocationsIdSet.Create(command.LocationIds);
        IEnumerable<Location> locations = await _locationsRepository.GetBySet(locationIds, ct);
        if (!locations.Any())
        {
            Error error = Error.NotFoundError("Локации для обновления подразделения не найдены.");
            return _logger.ReturnLogged(error);
        }

        Result<Department> department = await _departmentsRepository.GetById(
            command.DepartmentId,
            ct: ct
        );
        if (department.IsFailure)
        {
            return _logger.ReturnLogged<Guid>(department.Error);
        }

        Result updating = department.Value.UpdateLocations(locations);
        if (updating.IsFailure)
        {
            return _logger.ReturnLogged<Guid>(updating.Error);
        }

        await _departmentsRepository.Update(department.Value, ct);
        await _departmentLocationsRepository.DeleteByDepartmentId(command.DepartmentId, ct);

        foreach (DepartmentLocation departmentLocation in department.Value.Locations)
        {
            await _departmentLocationsRepository.Add(departmentLocation, ct);
        }

        Result committing = await transaction.CommitChanges(nameof(UpdateDepartmentLocationsCommand), ct);

        return committing.IsFailure
            ? _logger.ReturnLogged<Guid>(committing.Error)
            : department.Value.Id.Value;
    }
}
