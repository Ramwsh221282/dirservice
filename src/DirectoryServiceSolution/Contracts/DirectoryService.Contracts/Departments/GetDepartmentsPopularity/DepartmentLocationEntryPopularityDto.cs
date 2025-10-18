namespace DirectoryService.Contracts.Departments.GetDepartmentsPopularity;

public sealed class DepartmentLocationEntryPopularityDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string FullPath { get; init; }
    public required string TimeZone { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
