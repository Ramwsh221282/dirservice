using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;

namespace DirectoryService.Core.DeparmentsContext.Entities;

public sealed class DepartmentLocation
{
    public DepartmentId DepartmentId { get; }
    public Department Department { get; } = null!;
    public LocationId LocationId { get; }
    public Location Location { get; } = null!;

    private DepartmentLocation()
    {
        // ef core
    }

    public DepartmentLocation(Department department, Location location)
    {
        Department = department;
        Location = location;
        DepartmentId = department.Id;
        LocationId = location.Id;
    }
}
