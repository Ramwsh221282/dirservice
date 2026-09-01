namespace DirectoryService.UseCases.Departments.Contracts;

public sealed class DepartmentPositionSpecification
{
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }

    public bool IsEmpty => DepartmentId == null && PositionId == null;

    public DepartmentPositionSpecification()
    {
        DepartmentId = null;
        PositionId = null;
    }

    public DepartmentPositionSpecification WithDepartmentId(Guid departmentId)
    {
        DepartmentId = departmentId;
        return this;
    }

    public DepartmentPositionSpecification WithPositionId(Guid positionId)
    {
        PositionId = positionId;
        return this;
    }
}
