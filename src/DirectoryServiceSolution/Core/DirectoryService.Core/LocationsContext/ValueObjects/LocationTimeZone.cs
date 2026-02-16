using DirectoryService.Core.Common.Extensions;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationTimeZone
{
    public const int MaxLength = 100;
    public string Value { get; }

    private LocationTimeZone(string value)
    {
        Value = value;
    }

    public static Result<LocationTimeZone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string message = "Временная зона IANA была пустой.";
            return Error.ValidationError(message);
        }

        if (value.GreaterThan(MaxLength))
        {
            string message = "Временная зона IANA некорректна.";
            return Error.ValidationError(message);
        }

        string[] parts = value.Split('/');
        if (parts.Length != 2)
        {
            string message = "Временная зона IANA некорректна.";
            return Error.ValidationError(message);
        }

        parts[0] = parts[0].FormatForName();
        parts[1] = parts[1].FormatForName();
        return new LocationTimeZone(string.Join('/', parts));
    }
}
