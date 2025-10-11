using DirectoryService.Core.Common.Extensions;
using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.PositionsContext;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext;

public sealed class Department : ISoftDeletable
{
    private readonly List<DepartmentLocation> _locations = [];
    private readonly List<DepartmentPosition> _positions = [];
    public DepartmentId Id { get; }
    public DepartmentIdentifier Identifier { get; private set; } = null!;
    public EntityLifeCycle LifeCycle { get; private set; }
    public DepartmentName Name { get; private set; } = null!;
    public DepartmentPath Path { get; private set; } = null!;
    public DepartmentDepth Depth { get; private set; }
    public DepartmentId? Parent { get; private set; }
    public DepartmentChildrensCount ChildrensCount { get; private set; }
    public IReadOnlyList<DepartmentLocation> Locations => _locations;
    public IReadOnlyList<DepartmentPosition> Positions => _positions;

    public DepartmentChildAttachmentsHistory Attachments { get; private set; } =
        DepartmentChildAttachmentsHistory.Empty();

    public bool Deleted => LifeCycle.IsDeleted;

    private Department() { }

    private Department(
        DepartmentId id,
        DepartmentIdentifier identifier,
        EntityLifeCycle lifeCycle,
        DepartmentName name,
        DepartmentPath path,
        DepartmentDepth depth,
        DepartmentChildrensCount childrensCount,
        DepartmentChildAttachmentsHistory attachments,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions,
        DepartmentId? parent = null
    )
    {
        Id = id;
        Identifier = identifier;
        LifeCycle = lifeCycle;
        Name = name;
        Path = path;
        Depth = depth;
        Parent = parent;
        ChildrensCount = childrensCount;
        Attachments = attachments;
        _locations = [.. locations];
        _positions = [.. positions];
    }

    private Department(
        DepartmentId id,
        DepartmentIdentifier identifier,
        EntityLifeCycle lifeCycle,
        DepartmentName name,
        DepartmentPath path,
        DepartmentDepth depth,
        DepartmentChildrensCount childrensCount,
        DepartmentId? parent = null
    )
    {
        Id = id;
        Identifier = identifier;
        LifeCycle = lifeCycle;
        Name = name;
        Path = path;
        Depth = depth;
        Parent = parent;
        ChildrensCount = childrensCount;
        _locations = [];
        _positions = [];
        Attachments = DepartmentChildAttachmentsHistory.Empty();
    }

    public Result Detach(Department child)
    {
        DepartmentChildAttachmentsHistory detached = Attachments.Detach(child);

        Result<DepartmentChildrensCount> reducing = ChildrensCount.Reduce();
        if (reducing.IsFailure)
            return reducing.Error;

        LifeCycle = LifeCycle.Update();
        Attachments = detached;
        ChildrensCount = reducing;
        return Result.Success();
    }

    public Result UpdateLocations(IEnumerable<Location> locations)
    {
        if (Deleted)
            return Error.EntityDeletedError();

        _locations.Clear();

        LifeCycle = LifeCycle.Update();
        return AddLocations(locations);
    }

    public bool Includes(Department department) =>
        Parent != null && Parent == department.Id && Path.ContainsIdentifier(department.Identifier);

    public Result AddLocations(IEnumerable<Location> locations)
    {
        if (Deleted)
            return Error.EntityDeletedError();

        Location[] duplicates = [.. locations.ExtractDuplicates(l => l.Id)];
        if (duplicates.Any())
        {
            string[] duplicateIdentifiers = [.. duplicates.Select(l => l.Id.Value.ToString())];
            string errorMessage = $"""
                Невозможно добавить локации в подразделение.
                Найдены дубликаты локаций: {string.Join(',', duplicateIdentifiers)}
                """;
            return Error.ConflictError(errorMessage);
        }

        _locations.AddRange(locations.Select(l => new DepartmentLocation(this, l)));
        LifeCycle.Update();
        return Result.Success();
    }

    public Result AddPosition(Position position)
    {
        if (Deleted)
            return Error.EntityDeletedError();

        if (_positions.Any(p => p.DepartmentId == Id && p.PositionId == position.Id))
            return Error.ConflictError(
                $"Позиция {position.Name.Value} уже есть у подразделения {Id.Value}."
            );

        _positions.Add(new DepartmentPosition(this, position));
        LifeCycle.Update();
        return Result.Success();
    }

    public Result AttachOtherDepartment(Department other)
    {
        if (Deleted)
            return Error.EntityDeletedError();

        if (Attachments.IsAttached(other))
            return Error.ConflictError(
                $"Подразделение {other.Identifier.Value} уже прикреплено к {Identifier.Value}."
            );

        Result<DepartmentPath> childPath = Path.BindWithOther(other);
        if (childPath.IsFailure)
            return childPath.Error;

        Result<DepartmentChildrensCount> nextCount = ChildrensCount.Add(Path, other);
        if (nextCount.IsFailure)
            return childPath.Error;

        Result<DepartmentDepth> childDepth = childPath.Value.CalculateDepth();
        if (childDepth.IsFailure)
            return childDepth.Error;

        DepartmentChildAttachment attachment = new DepartmentChildAttachment(
            other.Id,
            DateTime.UtcNow
        );

        Attachments = Attachments.Attach(attachment);

        ChildrensCount = nextCount.Value;
        other.Parent = Id;
        other.Path = childPath.Value;
        other.Depth = childDepth.Value;
        LifeCycle.Update();
        return Result.Success();
    }

    public static Result<Department> CreateNew(
        DepartmentName name,
        DepartmentIdentifier identifier,
        IEnumerable<Location> locations,
        Department? parent
    )
    {
        Department child = CreateNew(name, identifier);
        if (parent != null)
        {
            Result attaching = parent.AttachOtherDepartment(child);
            if (attaching.IsFailure)
                return attaching.Error;
        }

        Result addingLocations = child.AddLocations(locations);
        if (addingLocations.IsFailure)
            return addingLocations.Error;

        return child;
    }

    public static Department Create(
        DepartmentId id,
        Guid? parentId,
        DepartmentIdentifier identifier,
        DepartmentName name,
        DepartmentPath path,
        DepartmentDepth depth,
        DepartmentChildAttachmentsHistory attachments,
        DepartmentChildrensCount childrensCount,
        EntityLifeCycle lifeCycle
    )
    {
        DepartmentId? parent = parentId == null ? null : DepartmentId.Create(parentId.Value).Value;
        return new Department(
            id,
            identifier,
            lifeCycle,
            name,
            path,
            depth,
            childrensCount,
            attachments,
            [],
            [],
            parent
        );
    }

    public static Department CreateNew(DepartmentName name, DepartmentIdentifier identifier)
    {
        DepartmentId id = new DepartmentId();
        DepartmentPath path = new DepartmentPath(identifier);
        DepartmentDepth depth = new DepartmentDepth();
        EntityLifeCycle lifeCycle = new EntityLifeCycle();
        DepartmentChildrensCount childrensCount = new DepartmentChildrensCount();
        return new Department(id, identifier, lifeCycle, name, path, depth, childrensCount, null);
    }
}
