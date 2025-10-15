namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

public interface ILocationElement
{
    public string Value { get; }
    public string Type { get; }
    public string ShortValue { get; }
    public short AoLevel { get; }
}
