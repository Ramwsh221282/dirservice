using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.DeparmentsContext.ValueObjects;

namespace DirectoryService.Core.DeparmentsContext;

public sealed class Department : ISoftDeletable
{
    public DepartmentId Id { get; }
    public DepartmentIdentifier Identifier { get; private set; }
    public DepartmentLifeCycle LifeCycle { get; private set; }
    public DepartmentName Name { get; private set; }
    public DepartmentPath Path { get; private set; }
    public DepartmentDepth Depth { get; private set; }
    public DepartmentId? Parent { get; private set; }
    public bool Deleted { get; set; }

    private Department(
        DepartmentId id,
        DepartmentIdentifier identifier,
        DepartmentLifeCycle lifeCycle,
        DepartmentName name,
        DepartmentPath path,
        DepartmentDepth depth,
        DepartmentId? parent = null,
        bool isDeleted = false)
    {
        Id = id;
        Identifier = identifier;
        LifeCycle = lifeCycle;
        Name = name;
        Path = path;
        Depth = depth;
        Parent = parent;
        Deleted = isDeleted;
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

    private static Department Create(
        DepartmentId id,
        DepartmentIdentifier identifier,
        DepartmentLifeCycle lifeCycle,
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
                    parent,
                    false);
    }
}
