namespace DirectoryService.Core.LocationsContext.ValueObjects;

public readonly record struct LocationId
{
    public Guid Value { get; }

    public LocationId()
    {
        Value = Guid.NewGuid();
    }

    public LocationId(Guid value)
    {
        Value = value;
    }
}
