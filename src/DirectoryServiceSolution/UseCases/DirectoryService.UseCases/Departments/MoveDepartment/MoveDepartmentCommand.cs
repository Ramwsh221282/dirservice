using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Departments.MoveDepartment;

public sealed record MoveDepartmentCommand : ICommand<Guid>
{
    public Guid MoveToId { get; }
    public Guid MovableId { get; }

    public MoveDepartmentCommand(Guid moveToId, Guid movableId)
    {
        MoveToId = moveToId;
        MovableId = movableId;
    }
}
