using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentId
{
    public Guid Value { get; }

    public DepartmentId() => Value = Guid.NewGuid();

    private DepartmentId(Guid value) => Value = value;

    public static Result<DepartmentId> Create(Guid value)
    {
        Result<Guid> validGuid = value.ValidGuid();
        return validGuid.IsSuccess ? new DepartmentId(validGuid) : validGuid.Error;
    }

    public static Result<DepartmentId> Create(string value)
    {
        Result<Guid> validGuid = value.ValidGuid();
        return validGuid.IsSuccess ? new DepartmentId(validGuid) : validGuid.Error;
    }
}
