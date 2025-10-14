namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

public sealed class LocationElementAttribute : Attribute
{
    public short AoLevel { get; }

    public LocationElementAttribute(short aoLevel)
    {
        AoLevel = aoLevel;
    }
}
