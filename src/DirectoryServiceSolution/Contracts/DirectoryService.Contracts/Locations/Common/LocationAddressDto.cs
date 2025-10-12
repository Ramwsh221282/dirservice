namespace DirectoryService.Contracts.Locations.Common;

public sealed record LocationAddressDto(
    string Country,
    string Region,
    string City,
    string Building,
    IEnumerable<string> Additionals
);
