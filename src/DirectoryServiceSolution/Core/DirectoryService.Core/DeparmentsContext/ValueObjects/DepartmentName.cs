using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentName
{
    public const short MinLength = 3;
    public const short MaxLength = 150;
    public string Value { get; }

    private DepartmentName(string value) => Value = value;

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError("Название подразделения не может быть пустым"); 
        
        
        if (value.LessThan(MinLength))
            return Error.ValidationError(
                $"Название подразделения не может быть менее {MinLength} символов"
            );
        
        if (value.GreaterThan(MaxLength))
            return Error.ValidationError(
                $"Название подразделения не может быть более {MaxLength} символов"
            );
        
        return new DepartmentName(value);
    }
}
