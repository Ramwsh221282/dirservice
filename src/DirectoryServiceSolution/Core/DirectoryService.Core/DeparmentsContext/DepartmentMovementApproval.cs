using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext;

/// <summary>
/// Объект - подтверждение, что движение подразделений согласовано.
/// </summary>
public sealed class DepartmentMovementApproval
{
    private readonly Department _newParent;
    private readonly Department _movingChild;
    private readonly bool _approved;

    public DepartmentMovementApproval(Department newParent, Department movingChild, bool approved)
    {
        _newParent = newParent;
        _movingChild = movingChild;
        _approved = approved;
    }

    public Result Approve(DepartmentMovement movement)
    {
        if (_newParent.Id != movement.MovingTo.Id)
            return Error.ConflictError(
                "Не удается согласовать движение подразделения. Разные ID новых родительских подразделений."
            );
        if (_movingChild.Id != movement.Movable.Id)
            return Error.ConflictError(
                "Не удается согласовать движение подразделения. Разный ID движимого подразделения."
            );
        if (_approved == false)
            return Error.ConflictError(
                "Нельзя передвинуть подразделение в его дочернее подразделение."
            );
        return Result.Success();
    }
}