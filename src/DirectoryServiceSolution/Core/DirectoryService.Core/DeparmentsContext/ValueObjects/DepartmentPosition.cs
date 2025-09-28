using DirectoryService.Core.PositionsContext;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed class DepartmentPosition
{
    public Department Department { get; }
    public Position Position { get; }

    public DepartmentPosition(Department department, Position position)
    {
        Department = department;
        Position = position;
    }
}