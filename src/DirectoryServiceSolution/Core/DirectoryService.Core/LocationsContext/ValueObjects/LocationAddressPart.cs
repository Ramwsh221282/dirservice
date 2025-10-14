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
        // ef core
    }

    private LocationAddressPart(string name, string shortName, short aoLevel, string type)
    {
        Name = name;
        ShortName = shortName;
        AoLevel = aoLevel;
        Type = type;
    }

    public static Result<LocationAddressPart> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.ValidationError("Часть адреса локации была пустой.");

        if (value.GreaterThan(MaxLength))
            return Error.ValidationError(
                $"Часть адреса локации превышает длину {MaxLength} символов."
            );

        if (value.LessThan(MinLength))
            return Error.ValidationError($"Часть адреса локации менее длины {MinLength} символов.");

        return CreateFromMatch(value);
    }

    private static Result<LocationAddressPart> CreateFromMatch(string value)
    {
        Result<LocationElement> elementResult = LocationElement.Create(value);
        if (elementResult.IsFailure)
            return elementResult.Error;

        LocationElement element = elementResult;
        return new LocationAddressPart(
            element.Value,
            element.ShortValue,
            element.AoLevel,
            element.Type
        );
    }
}
