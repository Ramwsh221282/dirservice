namespace DirectoryService.Contracts.Departments.GetDepartmentsPopularity;

public sealed class DepartmentsPopularityAttachmentsDto
{
    public required Guid Id { get; init; }
    public required DateTime AttachedAt { get; init; }
}
