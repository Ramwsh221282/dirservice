using DirectoryService.Core.Common.Extensions;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationTimeZone
{
    public const int MaxLength = 100;
    public string Value { get; }

    public LocationTimeZone(string value)
    {
        Value = value;
    }

    public static LocationTimeZone Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Временная зона IANA была пустой.");
        if (value.GreaterThan(MaxLength))
            throw new ArgumentException("Временная зона IANA некорректна.");
        string[] parts = value.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("Временная зона IANA некорректна.");
        parts[0] = parts[0].FormatForName();
        parts[1] = parts[1].FormatForName();
        return new LocationTimeZone(string.Join('/', parts));
    }
}
