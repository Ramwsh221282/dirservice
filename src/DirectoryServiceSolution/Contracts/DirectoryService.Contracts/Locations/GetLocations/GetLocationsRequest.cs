namespace DirectoryService.Contracts.Locations;

public sealed record GetLocationsRequest(
    int? Page = null,
    int? PageSize = null,
    string? NameSort = null,
    DateTime? DateCreatedSort = null,
    string? NameSearch = null,
    bool? IsActive = null,
    IEnumerable<Guid>? DepartmentIds = null
);
