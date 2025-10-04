using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentDepth
{
    public short Value { get; }

    public DepartmentDepth() => Value = 1;

    private DepartmentDepth(short value) => Value = value;

    public bool DepthValidBy(DepartmentPath path, DepartmentIdentifier identifier) =>
        path.ContainsIdentifier(identifier);

    public static Result<DepartmentDepth> Create(short value) =>
        value <= 0
            ? Error.ValidationError("Глубина подразделений не может быть меньше либо равна 0")
            : new DepartmentDepth(value);

    public static Result<DepartmentDepth> Create(
        DepartmentPath path,
        DepartmentIdentifier identifier
    ) => path.Depth(identifier);
}
