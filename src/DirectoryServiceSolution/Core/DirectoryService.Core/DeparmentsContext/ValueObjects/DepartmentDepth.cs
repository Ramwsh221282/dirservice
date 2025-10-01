using DirectoryService.Core.Common.Exceptions;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentDepth
{
    public short Value { get; }

    public DepartmentDepth()
    {
        throw new ConstructorShallNotBeCalledException(nameof(DepartmentDepth));
    }

    private DepartmentDepth(short value)
    {
        Value = value;
    }

    public bool DepthValidBy(DepartmentPath path, DepartmentIdentifier identifier)
    {
        return path.ContainsIdentifier(identifier);
    }

    public static DepartmentDepth Create(short value)
    {
        if (value <= 0)
            throw new ArgumentException("Глубина подразделений не может быть меньше либо равна 0", nameof(value));
        return new DepartmentDepth(value);
    }

    public static DepartmentDepth Create(DepartmentPath path, DepartmentIdentifier identifier)
    {
        return path.Depth(identifier);
    }
}
