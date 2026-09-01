using DirectoryService.Core.Common.Extensions;
using DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext.ValueObjects;

public sealed record LocationAddressPart
{
    public const int MinLength = 2;
    public const int MaxLength = 120;

    public string Name { get; }
    public string ShortName { get; }
    public short AoLevel { get; }
    public string Type { get; }

    private LocationAddressPart()
    {
        Name = null!;
        ShortName = null!;
        Type = null!;
        // ef core
    }

    private LocationAddressPart(string name, string shortName, short aoLevel, string type)
    {
        Name = name;
        ShortName = shortName;
        AoLevel = aoLevel;
        Type = type;
    }

    public static LocationAddressPart Create(
        string name,
        string shortName,
        short aoLevel,
        string type
    )
    {
        return new LocationAddressPart(name, shortName, aoLevel, type);
    }

    public static Result<LocationAddressPart> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            string message = "Часть адреса локации была пустой.";
            return Error.ValidationError(message);
        }

        if (value.GreaterThan(MaxLength))
        {
            string message = $"Часть адреса локации превышает длину {MaxLength} символов.";
            return Error.ValidationError(message);
        }

        if (value.LessThan(MinLength))
        {
            string message = $"Часть адреса локации менее длины {MinLength} символов.";
            return Error.ValidationError(message);
        }

        return CreateFromMatch(value);
    }

    private static Result<LocationAddressPart> CreateFromMatch(string value)
    {
        Result<LocationElement> elementResult = LocationElement.Create(value);
        if (elementResult.IsFailure)
        {
            return elementResult.Error;
        }

        LocationElement element = elementResult;
        return new LocationAddressPart(
            element.Value,
            element.ShortValue,
            element.AoLevel,
            element.Type
        );
    }
}
