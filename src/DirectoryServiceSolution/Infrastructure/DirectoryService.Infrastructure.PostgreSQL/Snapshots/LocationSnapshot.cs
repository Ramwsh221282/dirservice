using DirectoryService.Core.LocationsContext;

namespace DirectoryService.Infrastructure.PostgreSQL.Snapshots;

public sealed record LocationSnapshot(
    Guid Id,
    LocationAddressSnapshot Address,
    string Name,
    string TimeZone,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
)
{
    public static LocationSnapshot FromLocation(Location location)
    {
        return new LocationSnapshot(
            location.Id.Value,
            LocationAddressSnapshot.FromAddress(location.Address),
            location.Name.Value,
            location.TimeZone.Value,
            location.LifeCycle.CreatedAt,
            location.LifeCycle.UpdatedAt,
            location.LifeCycle.DeletedAt
        );
    }
}
