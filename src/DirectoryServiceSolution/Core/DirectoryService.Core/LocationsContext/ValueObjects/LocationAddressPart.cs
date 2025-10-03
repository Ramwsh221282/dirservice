using DirectoryService.Core.Common.Extensions;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationAddressPart
{
    public const int MinLength = 2;
    public const int MaxLength = 120;
    public string Node { get; } = null!;

    private LocationAddressPart()
    {
        // ef core
    }

    private LocationAddressPart(string value)
    {
        Node = value;
    }

    public static LocationAddressPart Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"Часть адреса локации была пустой.");
        string formatted = value.FormatForName();
        if (formatted.GreaterThan(MaxLength))
            throw new ArgumentException($"Часть адреса локации превышает длину {MaxLength} символов.");
        if (formatted.GreaterThan(MinLength))
            throw new ArgumentException($"Часть адреса локации менее длины {MinLength} символов.");
        return new LocationAddressPart(formatted);
    }
}