namespace DirectoryService.Core.LocationsContext.ValueObjects;

public interface ILocationElement
{
    public string Value { get; }
    public string Type { get; }
    public string ShortValue { get; }
    public short AoLevel { get; }
}
