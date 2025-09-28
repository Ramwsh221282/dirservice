using DirectoryService.Core.Common.Interfaces;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.PositionsContext.ValueObjects;

namespace DirectoryService.Core.PositionsContext;

public sealed class Position : ISoftDeletable
{
    public PositionId Id { get; }
    public PositionName Name { get; private set; }
    public PositionDescription Description { get; private set; }
    public EntityLifeCycle LifeCycle { get; private set; }
    public bool Deleted => LifeCycle.IsDeleted;


    public Position(PositionId id, PositionName name, PositionDescription description, EntityLifeCycle? lifeCycle = null)
    {
        Id = id;
        Name = name;
        Description = description;
        LifeCycle = lifeCycle ?? new EntityLifeCycle();
    }
}
