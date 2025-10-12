using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.PositionsContext.ValueObjects;
using ResultLibrary;

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

    private Position(
        PositionId id,
        PositionName name,
        PositionDescription description,
        EntityLifeCycle lifeCycle,
        IEnumerable<DepartmentPosition> departments
    )
    {
        Id = id;
        Name = name;
        Description = description;
        LifeCycle = lifeCycle;
        _departments = [.. departments];
    }

    private Position(
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

    public static Result<Position> CreateNew(
        PositionName name,
        PositionDescription description,
        PositionNameUniquesness uniquesness
    )
    {
        if (!uniquesness.IsUnique(name))
            return uniquesness.NotUniqueNameError();

        EntityLifeCycle lifeCycle = new EntityLifeCycle();
        PositionId id = new PositionId();
        return new Position(name, description, lifeCycle, id);
    }

    public static Result<Position> CreateNew(
        PositionName name,
        PositionDescription description,
        PositionNameUniquesness uniquesness,
        IEnumerable<Department> departments
    )
    {
        Result<Position> position = CreateNew(name, description, uniquesness);
        if (position.IsFailure)
            return position.Error;

        IEnumerable<DepartmentPosition> departmentPositions = departments.Select(
            d => new DepartmentPosition(d, position)
        );
        position.Value._departments.AddRange(departmentPositions);
        return position;
    }

    public Result BindToDepartment(IEnumerable<Department> departments)
    {
        if (Deleted)
            return Error.EntityDeletedError();

        foreach (Department department in departments)
        {
            Result adding = department.AddPosition(this);
            if (adding.IsFailure)
                return adding.Error;
        }

        return Result.Success();
    }
}
