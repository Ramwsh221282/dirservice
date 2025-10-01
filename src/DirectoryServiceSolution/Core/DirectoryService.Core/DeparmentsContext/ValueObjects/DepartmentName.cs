using DirectoryService.Core.Common.Extensions;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentName
{
    public const short MinLength = 3;
    public const short MaxLength = 150;
    public string Value { get; }

    private DepartmentName(string value)
    {
        Value = value;
    }

    public static DepartmentName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Название подразделения не может быть пустым", nameof(value));
        string formatted = value.FormatForName();
        if (formatted.LessThan(MinLength))
            throw new ArgumentException($"Название подразделения не может быть менее {MinLength} символов", nameof(value));
        if (formatted.GreaterThan(MaxLength))
            throw new ArgumentException($"Название подразделения не может быть более {MaxLength} символов", nameof(value));
        return new DepartmentName(formatted);    
    }
}
