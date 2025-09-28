using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext.ValueObjects;

namespace DirectoryService.Core.LocationsContext;

public sealed class Location
{
    private readonly List<DepartmentLocation> _departments = [];
    public LocationId Id { get; }
    public LocationAddress Address { get; private set; }
    public LocationName Name { get; private set; }
    public LocationTimeZone TimeZone { get; private set; }
    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public Location(LocationAddress address, LocationName name, LocationTimeZone timeZone, IEnumerable<DepartmentLocation> departments, LocationId? id = null)
    : this(address, name, timeZone, id)
    {
        _departments = departments.ToList();
    }   

    public Location(LocationAddress address, LocationName name, LocationTimeZone timeZone, LocationId? id = null)
    {
        Id = id ?? new LocationId();
        Address = address;
        Name = name;
        TimeZone = timeZone;        
    }   
}
