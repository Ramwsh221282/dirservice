using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext;

/// <summary>
/// Движение подразделения группирует в себе два подразделения для операции движения.
/// </summary>
public sealed class DepartmentMovement
{
    /// <summary>
    /// Новый родитель
    /// </summary>
    public Department MovingTo { get; }

    /// <summary>
    /// Подразделение, которое нужно передвинуть.
    /// </summary>
    public Department Movable { get; }

    public DepartmentMovement(Department movingTo, Department movable)
    {
        MovingTo = movingTo;
        Movable = movable;
    }

    public Result PerformMovement(Department oldAncestor, DepartmentMovementApproval approval)
    {
        Result approve = approval.Approve(this);
        if (approve.IsFailure)
            return approve;

        Result detaching = oldAncestor.Detach(Movable);
        return detaching.IsFailure ? detaching.Error : MovingTo.AttachOtherDepartment(Movable);
    }
}
