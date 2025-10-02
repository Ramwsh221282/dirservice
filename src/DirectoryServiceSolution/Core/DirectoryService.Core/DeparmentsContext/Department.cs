using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;

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
    public IReadOnlyList<DepartmentLocation> Locations => _locations;
    public IReadOnlyList<DepartmentPosition> Positions => _positions;
    public bool Deleted => LifeCycle.IsDeleted;

    private Department() { }

    private Department(
        DepartmentId id,
        DepartmentIdentifier identifier,
        EntityLifeCycle lifeCycle,
        DepartmentName name,
        DepartmentPath path,
        DepartmentDepth depth,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions,
        DepartmentId? parent = null)
    {
        Id = id;
        Identifier = identifier;
        LifeCycle = lifeCycle;
        Name = name;
        Path = path;
        Depth = depth;
        Parent = parent;
        _locations = locations.ToList();
        _positions = positions.ToList();
    }

    public Department AttachOtherDepartment(Department other)
    {
        DepartmentIdentifier childIdentifier = Identifier.AttachChildIdentifier(other.Identifier);
        DepartmentPath childPath = Path.CreateNodePart(other.Path);
        DepartmentDepth depth = childPath.Depth();
        other.Path = childPath;
        other.Depth = depth;
        other.Identifier = childIdentifier;
        other.Parent = Id;
        return other;        
    }

    public static Department Create(
        DepartmentId id,
        DepartmentIdentifier identifier,
        EntityLifeCycle lifeCycle,
        DepartmentName name,
        DepartmentPath path,
        DepartmentDepth depth,
        DepartmentId? parent = null,
        bool isDeleted = false)
    {
        if (isDeleted && lifeCycle.DeletedAt.HasValue)
            throw new ArgumentException("Подразделение не может быть удалено, без указания даты удаления");
        if (!path.ContainsIdentifier(identifier))
            throw new ArgumentException("Путь подразделения не содержит узел по идентификатору");
        if (!depth.DepthValidBy(path, identifier))
            throw new ArgumentException("Глубина подразделения не соответствует уровню в пути подразделения");
        return new Department(
                    id,
                    identifier,
                    lifeCycle,
                    name,
                    path,
                    depth,
                    [],
                    [],
                    parent);
    }
}
