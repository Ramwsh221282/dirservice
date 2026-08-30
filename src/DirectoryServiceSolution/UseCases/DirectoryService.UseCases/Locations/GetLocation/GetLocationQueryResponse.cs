namespace DirectoryService.UseCases.Locations.GetLocation;

public sealed class GetLocationQueryResponse
{
    public Guid Id { get; }
    public string Name { get; }
    public string TimeZone { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
    public DateTime? DeletedAt { get; }
    public string AddressObject { get; }
    public IEnumerable<Guid> DepartmentIds { get; }

    public GetLocationQueryResponse(
        Guid id,
        string name,
        string timeZone,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        string addressObject,
        IEnumerable<Guid> departmentIds)
    {
        Id = id;
        Name = name;
        TimeZone = timeZone;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
        AddressObject = addressObject;
        DepartmentIds = departmentIds;
    }
}
