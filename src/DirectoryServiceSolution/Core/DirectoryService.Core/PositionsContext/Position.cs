using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.PositionsContext.ValueObjects;

namespace DirectoryService.Core.PositionsContext;

public sealed class Position : ISoftDeletable
{
    private readonly List<DepartmentPosition> _departments = [];
    public PositionId Id { get; }
    public PositionName Name { get; private set; } = null!;
    public PositionDescription Description { get; private set; } = null!;
    public EntityLifeCycle LifeCycle { get; private set; }
    public bool Deleted => LifeCycle.IsDeleted;
    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    private Position()
    {
        // ef core
    }

    public Position(
        PositionName name,
        PositionDescription description,
        IEnumerable<DepartmentPosition> departments,
        EntityLifeCycle? lifeCycle = null,
        PositionId? id = null
    )
        : this(name, description, lifeCycle, id)
    {
        _departments = departments.ToList();
    }

    public Position(
        PositionName name,
        PositionDescription description,
        EntityLifeCycle? lifeCycle = null,
        PositionId? id = null
    )
    {
        Id = id ?? new PositionId();
        Name = name;
        Description = description;
        LifeCycle = lifeCycle ?? new EntityLifeCycle();
    }
}
