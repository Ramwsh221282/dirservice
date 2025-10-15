using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Locations.GetLocations;

public sealed record GetLocationsQuery : IQuery<GetLocationsResponse>
{
    public int Page { get; }
    public int PageSize { get; }
    public string? NameSearch { get; }
    public bool? IsActive { get; }
    public IEnumerable<Guid>? DepartmentIds { get; }
    public IEnumerable<string>? SortOptions { get; }
    public string SortDirection { get; } = "ASC";

    public GetLocationsQuery(
        string? nameSearch,
        bool? isActive,
        IEnumerable<Guid>? departmentIds,
        int? page,
        int? pageSize,
        IEnumerable<string>? sortOptions,
        string? sortDirection
    )
    {
        SortOptions = sortOptions;
        if (sortDirection is "DESC")
            SortDirection = "DESC";
        Page = page ?? 1;
        PageSize = pageSize ?? 20;
        NameSearch = nameSearch;
        IsActive = isActive;
        DepartmentIds = departmentIds;
    }
}
