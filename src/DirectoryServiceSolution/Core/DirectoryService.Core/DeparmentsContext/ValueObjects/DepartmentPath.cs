using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentPath
{
    public string Value { get; }

    private DepartmentPath(string value) => Value = value;

    public DepartmentPath(DepartmentIdentifier identifier) => Value = identifier.Value;

    public bool ContainsIdentifier(DepartmentIdentifier identifier)
    {
        int identifierIndex = IndexOfIdentifier(identifier);
        return identifierIndex >= 0;
    }

    public Result<DepartmentDepth> CalculateDepth()
    {
        string[] parts = Value.Split('.');
        short nextDepth = (short)(parts.Length - 1);
        return DepartmentDepth.Create(nextDepth);
    }

    public Result<int> DepthLevel(DepartmentIdentifier identifier)
    {
        int identifierIndex = IndexOfIdentifier(identifier);
        return identifierIndex == -1
            ? Error.NotFoundError(
                $"Не удается получить уровень глубины для подразделения с идентификатором {identifier.Value}"
            )
            : identifierIndex += 1;
    }

    public Result<DepartmentDepth> Depth()
    {
        string[] parts = Value.Split('.');
        return DepartmentDepth.Create((short)parts.Length);
    }

    public Result<DepartmentDepth> Depth(DepartmentIdentifier name)
    {
        int level = DepthLevel(name);
        return DepartmentDepth.Create((short)level);
    }

    public static Result<DepartmentPath> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError("Начальный узел пути подразделения не может быть пустым");
        string formatted = value.Trim().ToLower();
        return new DepartmentPath(formatted);
    }

    public Result<DepartmentPath> BindWithOther(Department department)
    {
        return BindWithOther(department.Identifier);
    }

    public Result<DepartmentPath> BindWithOther(DepartmentIdentifier node)
    {
        if (Value.Contains(node.Value))
            return Error.ConflictError(
                $"Путь подразделения {Value} уже содержит узел {node.Value}."
            );

        string[] nodes = [Value, node.Value];
        string completeName = string.Join('.', nodes);
        return new DepartmentPath(completeName);
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
}
