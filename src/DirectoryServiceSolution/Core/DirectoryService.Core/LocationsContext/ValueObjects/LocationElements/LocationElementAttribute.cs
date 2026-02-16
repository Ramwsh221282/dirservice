namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

[AttributeUsage(AttributeTargets.Class)]
public sealed class LocationElementAttribute : Attribute
{
    public short AoLevel { get; }

    public LocationElementAttribute(short aoLevel)
    {
        AoLevel = aoLevel;
    }
}
