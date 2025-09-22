using DirectoryService.Core.Common.Extensions;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentIdentifier
{
    public const short MinLength = 3;
    public const short MaxLength = 150;
    public string Value { get; }

    private DepartmentIdentifier(string value)
    {
        Value = value;
    }

    public static DepartmentIdentifier Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Идентификатор департамента не должен быть пустым", nameof(value));
        string formatted = value.Trim();
        if (formatted.LessThan(MinLength))
            throw new ArgumentException($"Идентификатор департамента не должен быть менее {MinLength} символов", nameof(value));
        if (formatted.GreaterThan(MaxLength))
            throw new ArgumentException($"Идентификатор департамента не должен быть более {MaxLength} символов", nameof(value));
        if (!formatted.IsLatinOnly())
            throw new ArgumentException($"Идентификатор департамента должен быть толькой латиницей", nameof(value));
        return new DepartmentIdentifier(value.ToUpper());
    }
}
