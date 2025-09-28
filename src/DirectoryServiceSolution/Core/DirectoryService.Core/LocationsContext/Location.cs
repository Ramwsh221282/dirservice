using DirectoryService.Core.LocationsContext.ValueObjects;

namespace DirectoryService.Core.LocationsContext;

public sealed class Location
{
    public LocationId Id { get; }
    public LocationAddress Address { get; private set; }
    public LocationName Name { get; private set; }
    public LocationTimeZone TimeZone { get; private set; }

    public Location(LocationAddress address, LocationName name, LocationTimeZone timeZone, LocationId? id = null)
    {
        Id = id ?? new LocationId();
        Address = address;
        Name = name;
        TimeZone = timeZone;
    }   
}
