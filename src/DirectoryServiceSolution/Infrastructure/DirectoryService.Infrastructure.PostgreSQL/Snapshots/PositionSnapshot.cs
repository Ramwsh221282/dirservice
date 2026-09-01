using DirectoryService.Core.PositionsContext;

namespace DirectoryService.Infrastructure.PostgreSQL.Snapshots;

public sealed record PositionSnapshot(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
)
{
    public static PositionSnapshot FromPosition(Position position)
    {
        return new PositionSnapshot(
            position.Id.Value,
            position.Name.Value,
            position.Description.Value,
            position.LifeCycle.CreatedAt,
            position.LifeCycle.UpdatedAt,
            position.LifeCycle.DeletedAt
        );
    }
}
