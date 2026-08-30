namespace DirectoryService.UseCases.Positions.GetPosition;

public sealed class GetPositionQueryResponse
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
    public DateTime? DeletedAt { get; }
    public IEnumerable<Guid> DepartmentIds { get; }

    public GetPositionQueryResponse(
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        IEnumerable<Guid> departmentIds)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
        DepartmentIds = departmentIds;
    }
}
