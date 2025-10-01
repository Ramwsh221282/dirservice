namespace DirectoryService.Core.PositionsContext.ValueObjects;

public readonly record struct PositionId
{
    public Guid Value { get; }

    public PositionId()
    {
        Value = Guid.NewGuid();
    }

    public PositionId(Guid value)
    {
        Value = value;
    }
}

