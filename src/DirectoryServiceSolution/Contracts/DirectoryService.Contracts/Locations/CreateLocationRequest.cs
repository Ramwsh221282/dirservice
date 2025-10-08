namespace DirectoryService.Contracts.Locations;

public sealed record CreateLocationRequest(
    string Name,
    LocationAddressDto Address,
    string TimeZone
);
