namespace DirectoryService.UseCases.Departments.Contracts;

public sealed class DepartmentLocationSpecification
{
    public Guid? DepartmentId { get; set; }
    public Guid? LocationId { get; set; }

    public bool IsEmpty => DepartmentId == null && LocationId == null;

    public DepartmentLocationSpecification()
    {
        DepartmentId = null;
        LocationId = null;
    }

    public DepartmentLocationSpecification WithDepartmentId(Guid departmentId)
    {
        DepartmentId = departmentId;
        return this;
    }

    public DepartmentLocationSpecification WithLocationId(Guid locationId)
    {
        LocationId = locationId;
        return this;
    }
}
