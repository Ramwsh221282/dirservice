namespace DirectoryService.UseCases.Locations.Contracts;

public sealed class LocationSpecification
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? TimeZone { get; set; }
    public bool? IsDeleted { get; set; }

    public bool IsEmpty => Id == null && Name == null && TimeZone == null && IsDeleted == null;

    public LocationSpecification()
    {
        Id = null;
        Name = null;
        TimeZone = null;
        IsDeleted = null;
    }

    public LocationSpecification WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public LocationSpecification WithName(string name)
    {
        Name = name;
        return this;
    }

    public LocationSpecification WithTimeZone(string timeZone)
    {
        TimeZone = timeZone;
        return this;
    }

    public LocationSpecification WithIsDeleted(bool isDeleted)
    {
        IsDeleted = isDeleted;
        return this;
    }
}
