namespace DirectoryService.UseCases.Departments.Contracts;

public sealed class DepartmentSpecification
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Identifier { get; set; }
    public string? Path { get; set; }
    public short? Depth { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsDeleted { get; set; }

    public bool IsEmpty =>
        Id == null
        && Name == null
        && Identifier == null
        && Path == null
        && Depth == null
        && ParentId == null
        && IsDeleted == null;

    public DepartmentSpecification()
    {
        Id = null;
        Name = null;
        Identifier = null;
        Path = null;
        Depth = null;
        ParentId = null;
        IsDeleted = null;
    }

    public DepartmentSpecification WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public DepartmentSpecification WithName(string name)
    {
        Name = name;
        return this;
    }

    public DepartmentSpecification WithIdentifier(string identifier)
    {
        Identifier = identifier;
        return this;
    }

    public DepartmentSpecification WithPath(string path)
    {
        Path = path;
        return this;
    }

    public DepartmentSpecification WithDepth(short depth)
    {
        Depth = depth;
        return this;
    }

    public DepartmentSpecification WithParentId(Guid parentId)
    {
        ParentId = parentId;
        return this;
    }

    public DepartmentSpecification WithIsDeleted(bool isDeleted)
    {
        IsDeleted = isDeleted;
        return this;
    }
}
