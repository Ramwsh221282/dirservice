using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Common.Transaction;
using DirectoryService.UseCases.Common.UnitOfWork;
using DirectoryService.UseCases.Departments.Contracts;
using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;
using Serilog;

namespace DirectoryService.UseCases.Departments.MoveDepartment;

public sealed class MoveDepartmentCommandHandler : ICommandHandler<Guid, MoveDepartmentCommand>
{
    private readonly ITransactionSource _transactions;
    private readonly IDepartmentsRepository _departments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ILogger _logger;

    public MoveDepartmentCommandHandler(
        ITransactionSource transactions,
        IDepartmentsRepository departments,
        IUnitOfWork unitOfWork,
        IValidator<MoveDepartmentCommand> validator,
        ILogger logger
    )
    {
        _transactions = transactions;
        _departments = departments;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        MoveDepartmentCommand command,
        CancellationToken ct = default
    )
    {
        await using ITransactionScope transaction = await _transactions.ReceiveTransaction(ct);

        ValidationResult validation = await _validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            Result<Guid> fail = validation.AsFailureResult<Guid>();
            return _logger.ReturnLogged(fail);
        }

        // получаем то, что хотим передвинуть и куда хотим передвинуть.
        Result<DepartmentMovement> movement = await _departments.GetDepartmentMovement(
            command.MoveToId,
            command.MovableId,
            ct
        );
        if (movement.IsFailure)
            return _logger.ReturnLogged<Guid>(movement.Error);

        // закрепляем для change tracker, потому что PerformMovement() доменная логика, изменяющая состояние объектов без SQL.
        _departments.Attach(movement.Value.Movable, movement.Value.MovingTo);

        // копируем старый путь подразделения, которое двигаем, чтобы дальше по нему расчитался ltree для дочерних подразделений после передвижения.
        DepartmentPath path = DepartmentPath.Create(movement.Value.Movable.Path.Value);

        // получаем текущего владельца подразделения, чтобы у него удалить из ChildAttachmentsHistory информацию о подразделении, которое движетя.
        Result<Department> oldAncestor = await _departments.GetParentDeparmentByChildPath(path, ct);
        if (oldAncestor.IsFailure)
            return _logger.ReturnLogged<Guid>(oldAncestor.Error);

        // процесс передвижения подразделения в качестве логики домена
        Result moving = movement.Value.PerformMovement(oldAncestor);
        if (moving.IsFailure)
            return _logger.ReturnLogged<Guid>(moving.Error);

        // сохранение изменений после логики домена для change tracker
        Result saving = await _unitOfWork.SaveChanges(ct);
        if (saving.IsFailure)
            return _logger.ReturnLogged<Guid>(saving.Error);

        // обновление пути у дочерних подразделений движимого подразделения.
        await _departments.RefreshDepartmentChildPaths(movement.Value.Movable, path, ct);

        Result committing = await transaction.CommitChanges(ct);
        if (committing.IsFailure)
            return _logger.ReturnLogged<Guid>(committing.Error);

        _logger.Information(
            "Department {Id} has been moved to {NewParentId}",
            movement.Value.Movable.Id.Value,
            movement.Value.MovingTo.Id.Value
        );

        return movement.Value.Movable.Id.Value;
    }
}
