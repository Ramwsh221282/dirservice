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
        {
            string message = "Не удается согласовать движение подразделения. Разные ID новых родительских подразделений.";
            return Error.ConflictError(message);
        }

        if (_movingChild.Id != movement.Movable.Id)
        {
            string message = "Не удается согласовать движение подразделения. Разный ID движимого подразделения.";
            return Error.ConflictError(message);
        }

        if (!_approved)
        {
            string message = "Нельзя передвинуть подразделение в его дочернее подразделение.";
            return Error.ConflictError(message);
        }

        return Result.Success();
    }
}