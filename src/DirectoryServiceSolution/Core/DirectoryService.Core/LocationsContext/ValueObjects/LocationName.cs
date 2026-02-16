using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationName
{
    public const int MinLength = 2;
    public const int MaxLength = 120;
    public string Value { get; }

    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string message = $"Наименование локации было пустым.";
            return Error.ValidationError(message);
        }

        if (value.GreaterThan(MaxLength))
        {
            string message = $"Наименование превышает длину {MaxLength} символов.";
            return Error.ValidationError(message);
        }

        if (value.LessThan(MinLength))
        {
            string message = $"Наименование менее длины {MinLength} символов.";
            return Error.ValidationError(message);
        }

        return new LocationName(value);
    }
}