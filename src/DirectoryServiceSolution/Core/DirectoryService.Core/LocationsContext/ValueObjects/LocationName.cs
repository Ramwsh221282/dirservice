using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationName
{
    public const int MinLength = 2;
    public const int MaxLength = 120;
    public string Value { get; }

    private LocationName(string value) => Value = value;

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError($"Наименование локации было пустым.");

        if (value.GreaterThan(MaxLength))
            return Error.ValidationError($"Наименование превышает длину {MaxLength} символов.");

        if (value.LessThan(MinLength))
            return Error.ValidationError($"Наименование менее длины {MinLength} символов.");

        return new LocationName(value);
    }
}