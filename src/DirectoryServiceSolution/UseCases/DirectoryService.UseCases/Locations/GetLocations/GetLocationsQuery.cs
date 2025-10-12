using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Locations.GetLocations;

public sealed record GetLocationsQuery : IQuery<GetLocationsResponse>
{
    public int Page { get; }
    public int PageSize { get; }
    public string? NameSortMode { get; }
    public string? DateCreatedSortMode { get; }
    public string? NameSearch { get; }
    public bool? IsActive { get; }
    public IEnumerable<Guid>? DepartmentIds { get; }

    public GetLocationsQuery(
        string? nameSortMode,
        string? dateCreatedSortMode,
        string? nameSearch,
        bool? isActive,
        IEnumerable<Guid>? departmentIds,
        int? page,
        int? pageSize
    )
    {
        NameSortMode = nameSortMode;
        DateCreatedSortMode = dateCreatedSortMode;
        Page = page ?? 1;
        PageSize = pageSize ?? 20;
        NameSearch = nameSearch;
        IsActive = isActive;
        DepartmentIds = departmentIds;
    }
}
