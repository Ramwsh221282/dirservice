using DirectoryService.Core.Common.Extensions;

namespace DirectoryService.Core.PositionsContext.ValueObjects;

public sealed record PositionDescription
{
    public const int MinLength = 3;
    public const int MaxLength = 1000;
    public string Value { get; }

    private PositionDescription(string value)
    {
        Value = value;
    }

    public static PositionDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Описание должности не должно быть пустым.");
        string formatted = value.MakeFirstLetterCapital();
        if (formatted.GreaterThan(MaxLength))
            throw new ArgumentException($"Описание превышает длину {MaxLength} символов.");
        if (formatted.LessThan(MinLength))
            throw new ArgumentException($"Описание менее {MinLength} символов.");
        return new PositionDescription(value);
    }
}

