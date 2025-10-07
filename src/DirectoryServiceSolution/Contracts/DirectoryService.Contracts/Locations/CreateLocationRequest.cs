namespace DirectoryService.Contracts.Locations;

public sealed record CreateLocationRequest(
    LocationNameDto Name,
    IEnumerable<LocationAddressNodeDto> AddressNodes,
    LocationTimeZoneDto TimeZone
);
