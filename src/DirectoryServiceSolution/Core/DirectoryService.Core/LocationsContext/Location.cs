using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext.ValueObjects;

namespace DirectoryService.Core.LocationsContext;

public sealed class Location : ISoftDeletable
{
    private readonly List<DepartmentLocation> _departments = [];
    public LocationId Id { get; }
    public LocationAddress Address { get; private set; }
    public LocationName Name { get; private set; }
    public LocationTimeZone TimeZone { get; private set; }
    public EntityLifeCycle LifeCycle { get; private set; }
    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public bool Deleted => LifeCycle.IsDeleted;

    public Location(LocationAddress address, LocationName name, LocationTimeZone timeZone, IEnumerable<DepartmentLocation> departments, LocationId? id = null, EntityLifeCycle? lifeCycle = null)
    : this(address, name, timeZone, id, lifeCycle)
    {
        _departments = [.. departments];        
    }

    public Location(LocationAddress address, LocationName name, LocationTimeZone timeZone, LocationId? id = null, EntityLifeCycle? lifeCycle = null)
    {
        Id = id ?? new LocationId();
        Address = address;
        Name = name;
        TimeZone = timeZone;        
        LifeCycle = lifeCycle ?? new EntityLifeCycle();
    }   
}
