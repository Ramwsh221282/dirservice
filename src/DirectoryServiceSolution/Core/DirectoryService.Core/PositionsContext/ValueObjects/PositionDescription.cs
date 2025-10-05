using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.PositionsContext.ValueObjects;

public sealed record PositionDescription
{
    public const int MinLength = 3;
    public const int MaxLength = 1000;
    public string Value { get; }

    private PositionDescription(string value) => Value = value;

    public static Result<PositionDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError($"Описание должности не должно быть пустым.");

        string formatted = value.MakeFirstLetterCapital();
        if (formatted.GreaterThan(MaxLength))
            return Error.ValidationError($"Описание превышает длину {MaxLength} символов.");

        if (formatted.LessThan(MinLength))
            return Error.ValidationError($"Описание менее {MinLength} символов.");

        return new PositionDescription(value);
    }
}
