namespace DirectoryService.Core.LocationsContext.ValueObjects.LocationElements;

public interface ILocationElement
{
    string Value { get; }
    string Type { get; }
    string ShortValue { get; }
    short AoLevel { get; }
}
