using DirectoryService.Core.Common.Exceptions;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentId
{
    public Guid Value { get; }

    public DepartmentId()
    {
        throw new ConstructorShallNotBeCalledException(nameof(DepartmentId));
    }

    private DepartmentId(Guid value)
    {
        Value = value;
    }

    public static DepartmentId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Идентификатор был пустым", nameof(value));
        return new DepartmentId(value);
    }

    public static DepartmentId Create(string value)
    {
        bool isGuid = Guid.TryParse(value, out Guid guidValue);
        if (!isGuid) throw new ArgumentException("Идентификатор не является форматом GUID", nameof(value));
        return Create(guidValue);
    }
}
