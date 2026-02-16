using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentDepth
{
    public short Value { get; }

    public DepartmentDepth()
    {
        Value = 0;
    }

    private DepartmentDepth(short value)
    {
        Value = value;
    }

    public static Result<DepartmentDepth> Create(short value)
    {
        if (value < 0)
        {
            string message = "Глубина подразделений не может быть отрицательной 0";
            return Error.ValidationError(message);
        }

        return new DepartmentDepth(value);
    }
}
