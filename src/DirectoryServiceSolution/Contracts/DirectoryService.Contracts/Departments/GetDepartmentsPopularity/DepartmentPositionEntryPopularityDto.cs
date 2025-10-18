namespace DirectoryService.Contracts.Departments.GetDepartmentsPopularity;

public sealed class DepartmentPositionEntryPopularityDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required string Description { get; init; }
}
