using DirectoryService.Core.Common.Extensions;

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

    public static LocationName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Наименование локации было пустым.");
        string formatted = value.FormatForName();
        if (formatted.GreaterThan(MaxLength))
            throw new ArgumentException($"Наименование превышает длину {MaxLength} символов.");
        if (formatted.LessThan(MinLength))
            throw new ArgumentException($"Наименование менее длины {MinLength} символов.");
        return new LocationName(formatted);
    }
}
