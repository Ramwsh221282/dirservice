using DirectoryService.Contracts.Locations.Common;

namespace DirectoryService.Contracts.Locations.CreateLocation;

public sealed record CreateLocationRequest(
    string Name,
    LocationAddressDto Address,
    string TimeZone
);
