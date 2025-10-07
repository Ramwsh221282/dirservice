using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentDepth
{
    public short Value { get; }

    public DepartmentDepth() => Value = 0;

    private DepartmentDepth(short value) => Value = value;

    public static Result<DepartmentDepth> Create(short value) =>
        value < 0
            ? Error.ValidationError("Глубина подразделений не может быть отрицательной 0")
            : new DepartmentDepth(value);
}
