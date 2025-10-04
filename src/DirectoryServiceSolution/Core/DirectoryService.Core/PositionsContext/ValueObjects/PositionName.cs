using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.PositionsContext.ValueObjects;

public sealed record PositionName
{
    public const int MaxLength = 100;
    public const int MinLength = 3;
    public string Value { get; }

    private PositionName(string value) => Value = value;

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError($"Наименование должности не должно быть пустым.");

        string formatted = value.FormatForName();
        if (formatted.GreaterThan(MaxLength))
            return Error.ValidationError($"Наименование превышает длину {MaxLength} символов.");

        if (formatted.LessThan(MinLength))
            return Error.ValidationError($"Наименование менее {MinLength} символов.");

        return new PositionName(value);
    }
}
