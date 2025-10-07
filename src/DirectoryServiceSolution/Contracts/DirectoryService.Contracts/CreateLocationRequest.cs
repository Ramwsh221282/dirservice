namespace DirectoryService.Contracts;

public sealed record CreateLocationRequest(
    LocationNameDto Name,
    IEnumerable<LocationAddressNodeDto> AddressNodes,
    LocationTimeZoneDto TimeZone
);
