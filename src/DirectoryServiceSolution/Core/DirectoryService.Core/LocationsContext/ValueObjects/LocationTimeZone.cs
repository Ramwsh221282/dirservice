using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationTimeZone
{
    public const int MaxLength = 100;
    public string Value { get; }

    private LocationTimeZone(string value) => Value = value;

    public static Result<LocationTimeZone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError("Временная зона IANA была пустой.");

        if (value.GreaterThan(MaxLength))
            return Error.ValidationError("Временная зона IANA некорректна.");

        string[] parts = value.Split('/');
        if (parts.Length != 2)
            return Error.ValidationError("Временная зона IANA некорректна.");

        parts[0] = parts[0].FormatForName();
        parts[1] = parts[1].FormatForName();
        return new LocationTimeZone(string.Join('/', parts));
    }
}
