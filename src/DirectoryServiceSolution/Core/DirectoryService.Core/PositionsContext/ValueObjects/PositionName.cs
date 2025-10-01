using DirectoryService.Core.Common.Extensions;

namespace DirectoryService.Core.PositionsContext.ValueObjects;

public sealed record PositionName
{
    public const int MaxLength = 100;
    public const int MinLength = 3;
    public string Value { get; }

    private PositionName(string value)
    {
        Value = value;
    }

    public static PositionName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Наименование должности не должно быть пустым.");
        string formatted = value.FormatForName();
        if (formatted.GreaterThan(MaxLength))
            throw new ArgumentException($"Наименование превышает длину {MaxLength} символов.");
        if (formatted.LessThan(MinLength))
            throw new ArgumentException($"Наименование менее {MinLength} символов.");
        return new PositionName(value);
    }
}

