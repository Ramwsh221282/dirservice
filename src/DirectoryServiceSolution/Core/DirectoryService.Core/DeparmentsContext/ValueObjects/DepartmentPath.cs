namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentPath
{
    public string Value { get; }

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public DepartmentPath CreateNodePart(DepartmentPath other)
    {
        return Create(this, other);
    }

    public bool ContainsIdentifier(DepartmentIdentifier identifier)
    {
        int identifierIndex = IndexOfIdentifier(identifier);
        return identifierIndex >= 0;
    }

    public int DepthLevel(DepartmentIdentifier identifier)
    {
        int identifierIndex = IndexOfIdentifier(identifier);
        return identifierIndex == -1 ?
            throw new ApplicationException($"Не удается получить уровень глубины для подразделения с идентификатором {identifier.Value}") :
            identifierIndex += 1;
    }

    public DepartmentDepth Depth()
    {
        string[] parts = Value.Split('.');
        return DepartmentDepth.Create((short)parts.Length);
    }

    public DepartmentDepth Depth(DepartmentIdentifier name)
    {
        int level = DepthLevel(name);
        return DepartmentDepth.Create((short)level);
    }

    public DepartmentPath CreateNodePart(string other)
    {
        DepartmentPath node = Create(other);
        return CreateNodePart(node);
    }    

    private int IndexOfIdentifier(DepartmentIdentifier identifier)
    {
        string[] parts = SplitNames();
        for (int idx = 0; idx < parts.Length; idx++)
        {
            if (string.Equals(parts[idx], identifier.Value, StringComparison.OrdinalIgnoreCase))
                return idx;
        }

        return -1;
    }

    private string[] SplitNames()
    {
        return Value.Split('.');
    }

    public static DepartmentPath Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Начальный узел пути подразделения не может быть пустым");
        string formatted = value.Trim().ToLower();
        return new DepartmentPath(formatted);
    }
    
    public static DepartmentPath Create(DepartmentPath parentPath, string value)
    {
        DepartmentPath node = Create(value);
        return Create(parentPath, node);
    }

    public static DepartmentPath Create(DepartmentPath parentPath, DepartmentPath node)
    {
        string[] nodes = [parentPath.Value, node.Value];
        string completeName = string.Join('.', nodes);
        return new DepartmentPath(completeName);
    }
}
