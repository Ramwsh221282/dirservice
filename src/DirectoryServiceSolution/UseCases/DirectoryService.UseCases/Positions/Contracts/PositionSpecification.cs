namespace DirectoryService.UseCases.Positions.Contracts;

public sealed class PositionSpecification
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeleted { get; set; }

    public bool IsEmpty => Id == null && Name == null && Description == null && IsDeleted == null;

    public PositionSpecification()
    {
        Id = null;
        Name = null;
        Description = null;
        IsDeleted = null;
    }

    public PositionSpecification WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public PositionSpecification WithName(string name)
    {
        Name = name;
        return this;
    }

    public PositionSpecification WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public PositionSpecification WithIsDeleted(bool isDeleted)
    {
        IsDeleted = isDeleted;
        return this;
    }
}
