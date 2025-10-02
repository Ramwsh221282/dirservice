using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;

namespace DirectoryService.Core.DeparmentsContext.Entities;

public sealed class DepartmentPosition
{
    public DepartmentId DepartmentId { get; }
    public Department Department { get; } = null!;
    public PositionId PositionId { get; }
    public Position Position { get; } = null!;

    private DepartmentPosition() { }

    public DepartmentPosition(Department department, Position position)
    {
        Department = department;
        Position = position;
        DepartmentId = department.Id;
        PositionId = position.Id;
    }
}
