using System.Globalization;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentPath
{
    private const char Separator = '.';
    public string Value { get; }

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public DepartmentPath(DepartmentIdentifier identifier)
    {
        Value = identifier.Value;
    }

    public DepartmentPath Copy()
    {
        return new DepartmentPath(Value);
    }

    public bool ContainsIdentifier(DepartmentIdentifier identifier)
    {
        int identifierIndex = IndexOfIdentifier(identifier);
        return identifierIndex >= 0;
    }

    public Result<DepartmentDepth> CalculateDepth()
    {
        string[] parts = Value.Split(Separator);
        short nextDepth = (short)(parts.Length - 1);
        return DepartmentDepth.Create(nextDepth);
    }

    public Result<int> DepthLevel(DepartmentIdentifier identifier)
    {
        int identifierIndex = IndexOfIdentifier(identifier);
        if (identifierIndex == -1)
        {
            string message = $"Не удается получить уровень глубины для подразделения с идентификатором {identifier.Value}";
            return Error.NotFoundError(message);
        }

        return identifierIndex + 1;
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
        {
            string message = "Начальный узел пути подразделения не может быть пустым";
            return Error.ValidationError(message);
        }
        
        return new DepartmentPath(FormatDepartmentPathString(value));
    }

    public Result<DepartmentPath> BindWithOther(Department department)
    {
        return BindWithOther(department.Identifier);
    }

    public Result<DepartmentPath> BindWithOther(DepartmentIdentifier node)
    {
        if (NodeExistsInPath(this, node))
        {
            string message = $"Путь подразделения {Value} уже содержит узел {node.Value}.";
            return Error.ConflictError(message);
        }
    
        string[] nodes = [Value, node.Value];
        string completeName = string.Join(Separator, nodes);
        return new DepartmentPath(completeName);
    }

    private static string FormatDepartmentPathString(string input)
    {
        string formatted = input.Trim().ToLower(CultureInfo.InvariantCulture);
        return formatted;
    }

    private int IndexOfIdentifier(DepartmentIdentifier identifier)
    {
        string[] parts = SplitNames();
        for (int idx = 0; idx < parts.Length; idx++)
        {
            if (string.Equals(parts[idx], identifier.Value, StringComparison.OrdinalIgnoreCase))
            {
                return idx;
            }
        }

        return -1;
    }

    private static bool NodeExistsInPath(DepartmentPath path, DepartmentIdentifier node)
    {
        string pathString = path.Value;
        string nodeString = node.Value;
        return pathString.Contains(nodeString, StringComparison.Ordinal);
    }

    private string[] SplitNames()
    {
        return Value.Split(Separator);
    }
}
