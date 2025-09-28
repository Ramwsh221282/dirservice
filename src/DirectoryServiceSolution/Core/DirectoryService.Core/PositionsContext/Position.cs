using DirectoryService.Core.PositionsContext.ValueObjects;

namespace DirectoryService.Core.PositionsContext;

public sealed class Position
{
    public PositionId Id { get; }
    public PositionName Name { get; private set; }
    public PositionDescription Description { get; private set; }

    public Position(PositionId id, PositionName name, PositionDescription description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
}
