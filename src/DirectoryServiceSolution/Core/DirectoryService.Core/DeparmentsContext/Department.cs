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
    public DepartmentId? Parent { get; private set; } = null!;
    public DepartmentChildrensCount ChildrensCount { get; private set; }
    public IReadOnlyList<DepartmentLocation> Locations => _locations;
    public IReadOnlyList<DepartmentPosition> Positions => _positions;
    public DepartmentAttachmentsHistory Attachments { get; private set; } =
        DepartmentAttachmentsHistory.Empty();
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
        DepartmentAttachmentsHistory attachments,
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
        Attachments = DepartmentAttachmentsHistory.Empty();
    }

    public bool Includes(Department department) =>
        Parent != null && Parent == department.Id && Path.ContainsIdentifier(department.Identifier);

    public Result AddLocations(IEnumerable<Location> locations)
    {
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
        if (_positions.Any(p => p.DepartmentId == Id && p.PositionId == position.Id))
            return Error.ConflictError(
                $"Позиция {position.Name.Value} уже есть у подразделения {Id.Value}."
            );
        _positions.Add(new DepartmentPosition(this, position));
        return Result.Success();
    }

    public Result AttachOtherDepartment(Department other)
    {
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
        Attachments = new DepartmentAttachmentsHistory([.. Attachments.Attachments, attachment]);

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
