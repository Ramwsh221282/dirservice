namespace DirectoryService.Contracts.Locations;

public sealed record CreateLocationRequest(
    string Name,
    LocationAddressNodeDto AddressNodes,
    string TimeZone
);
