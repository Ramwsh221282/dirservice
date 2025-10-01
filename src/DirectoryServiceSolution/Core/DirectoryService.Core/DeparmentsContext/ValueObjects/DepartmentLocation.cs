using DirectoryService.Core.LocationsContext;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed class DepartmentLocation
{
    public Department Department { get; }
    public Location Location { get; }

    public DepartmentLocation(Department department, Location location)
    {
        Department = department;
        Location = location;
    }
}
