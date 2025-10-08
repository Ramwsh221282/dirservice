using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Common.UnitOfWork;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Positions.Contracts;
using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;
using Serilog;

namespace DirectoryService.UseCases.Positions.CreatePosition;

public sealed class CreatePositionCommandHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IDepartmentsRepository _departments;
    private readonly IPositionsRepository _positions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger _logger;

    public CreatePositionCommandHandler(
        IDepartmentsRepository departments,
        IPositionsRepository positions,
        IUnitOfWork unitOfWork,
        IValidator<CreatePositionCommand> validator,
        ILogger logger
    )
    {
        _departments = departments;
        _positions = positions;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreatePositionCommand command,
        CancellationToken ct = default
    )
    {
        ValidationResult validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            Result<Guid> failed = validation.AsFailureResult<Guid>();
            _logger.Error(
                "{Method}. {Error}.",
                nameof(CreatePositionCommand),
                failed.Error.Message
            );
            return failed;
        }

        PositionName name = PositionName.Create(command.Name);
        PositionDescription description = PositionDescription.Create(command.Description);
        PositionNameUniquesness uniquesness = await _positions.IsUnique(name, ct);
        Result<Position> position = Position.CreateNew(name, description, uniquesness);
        if (position.IsFailure)
            return position.Error;

        await _positions.Add(position.Value, ct);

        DepartmentsIdSet identifiers = DepartmentsIdSet.Create(command.DepartmentIdentifiers);
        IEnumerable<Department> departments = await _departments.GetByIdArray(identifiers, ct);
        if (!departments.Any())
            return Error.ConflictError(
                "Не найдены подразделения, для которых нужно прикрепить позицию."
            );

        Result binding = position.Value.BindToDepartment(departments);
        if (binding.IsFailure)
            return binding.Error;

        Result saving = await _unitOfWork.SaveChanges(ct);
        return saving.IsFailure ? saving.Error : position.Value.Id.Value;
    }
}
