using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentChildAttachment(DepartmentId Id, DateTime AttachedAt)
{
    public DepartmentId Id { get; } = Id;
    public DateTime AttachedAt { get; } = AttachedAt;

    public static Result<DepartmentChildAttachment> Create(Guid id, DateTime attachedAt)
    {
        if (attachedAt == DateTime.MinValue || attachedAt == DateTime.MaxValue)
        {
            string message = "Некорректное время закрепления подразделения.";
            return Error.ValidationError(message);
        }

        Result<DepartmentId> departmentId = DepartmentId.Create(id);
        if (departmentId.IsFailure)
        {
            return departmentId.Error;
        }

        return new DepartmentChildAttachment(departmentId, attachedAt);
    }
}
