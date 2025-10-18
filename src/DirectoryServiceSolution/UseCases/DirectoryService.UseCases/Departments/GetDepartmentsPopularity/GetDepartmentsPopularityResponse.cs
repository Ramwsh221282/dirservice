using DirectoryService.Contracts.Departments.GetDepartmentsPopularity;

namespace DirectoryService.UseCases.Departments.GetDepartmentsPopularity;

public sealed class GetDepartmentsPopularityResponse
{
    public required Guid Id { get; init; }
    public required double PositionsPercentage { get; init; }
    public required double LocationsPercentage { get; init; }
    public required string Identifier { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required int Depth { get; init; }
    public required Guid? ParentId { get; init; }
    public required int ChildrensCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required IEnumerable<DepartmentsPopularityAttachmentsDto> Attachments { get; init; }
    public required IEnumerable<DepartmentPositionEntryPopularityDto> Positions { get; init; }
    public required IEnumerable<DepartmentLocationEntryPopularityDto> Locations { get; init; }
}
