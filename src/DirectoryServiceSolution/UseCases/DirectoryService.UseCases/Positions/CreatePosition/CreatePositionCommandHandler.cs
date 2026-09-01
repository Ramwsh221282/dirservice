using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Common.Transaction;
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
    private readonly IDepartmentPositionsRepository _departmentPositions;
    private readonly ITransactionSource _transactionSource;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger _logger;

    public CreatePositionCommandHandler(
        IDepartmentsRepository departments,
        IPositionsRepository positions,
        IDepartmentPositionsRepository departmentPositions,
        ITransactionSource transactionSource,
        IValidator<CreatePositionCommand> validator,
        ILogger logger
    )
    {
        _departments = departments;
        _positions = positions;
        _departmentPositions = departmentPositions;
        _transactionSource = transactionSource;
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

        await using ITransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);

        PositionName name = PositionName.Create(command.Name);
        PositionDescription description = PositionDescription.Create(command.Description);
        PositionNameUniquesness uniquesness = await _positions.IsUnique(name, ct);
        Result<Position> position = Position.CreateNew(name, description, uniquesness);
        if (position.IsFailure)
        {
            return position.Error;
        }

        DepartmentsIdSet identifiers = DepartmentsIdSet.Create(command.DepartmentIdentifiers);
        IEnumerable<Department> departments = await _departments.GetByIdArray(identifiers, ct);
        if (!departments.Any())
        {
            string message = "Не найдены подразделения, для которых нужно прикрепить позицию.";
            return Error.ConflictError(message);
        }

        Result binding = position.Value.BindToDepartment(departments);
        if (binding.IsFailure)
        {
            return binding.Error;
        }

        await _positions.Add(position.Value, ct);

        foreach (Department department in departments)
        {
            DepartmentPosition? departmentPosition = department.Positions.FirstOrDefault(p =>
                p.PositionId == position.Value.Id
            );

            if (departmentPosition != null)
            {
                await _departmentPositions.Add(departmentPosition, ct);
            }
        }

        Result committing = await transaction.CommitChanges(nameof(CreatePositionCommand), ct);
        if (committing.IsFailure)
        {
            return committing.Error;
        }

        return position.Value.Id.Value;
    }
}
