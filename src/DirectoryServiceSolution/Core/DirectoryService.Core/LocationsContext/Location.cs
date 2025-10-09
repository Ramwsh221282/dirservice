using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.LocationsContext.ValueObjects;
using ResultLibrary;

namespace DirectoryService.Core.LocationsContext;

public sealed class Location : ISoftDeletable
{
    private readonly List<DepartmentLocation> _departments = [];
    public LocationId Id { get; }
    public LocationAddress Address { get; private set; } = null!;
    public LocationName Name { get; private set; } = null!;
    public LocationTimeZone TimeZone { get; private set; } = null!;
    public EntityLifeCycle LifeCycle { get; private set; }
    public IReadOnlyList<DepartmentLocation> Departments => _departments;
    public bool Deleted => LifeCycle.IsDeleted;

    private Location()
    {
        // ef core
    }

    private Location(
        LocationAddress address,
        LocationName name,
        LocationTimeZone timeZone,
        IEnumerable<DepartmentLocation> departments,
        LocationId? id = null,
        EntityLifeCycle? lifeCycle = null
    )
        : this(address, name, timeZone, id, lifeCycle)
    {
        _departments = [.. departments];
    }

    private Location(
        LocationAddress address,
        LocationName name,
        LocationTimeZone timeZone,
        LocationId? id = null,
        EntityLifeCycle? lifeCycle = null
    )
    {
        Id = id ?? new LocationId();
        Address = address;
        Name = name;
        TimeZone = timeZone;
        LifeCycle = lifeCycle ?? new EntityLifeCycle();
    }

    public static Result<Location> CreateNew(
        LocationName name,
        LocationAddress address,
        LocationTimeZone timeZone,
        LocationNameUniquesness uniquesness
    )
    {
        if (!uniquesness.IsUnique(name))
            return uniquesness.NonUniqueLocationError();

        LocationId id = new LocationId();
        EntityLifeCycle lifeCycle = new EntityLifeCycle();
        return new Location(address, name, timeZone, id, lifeCycle);
    }

    public static Location Create(
        LocationAddress address,
        LocationName name,
        LocationTimeZone timeZone,
        IEnumerable<DepartmentLocation> departments,
        LocationId? id = null,
        EntityLifeCycle? lifeCycle = null
    ) => new(address, name, timeZone, departments, id, lifeCycle);
}
