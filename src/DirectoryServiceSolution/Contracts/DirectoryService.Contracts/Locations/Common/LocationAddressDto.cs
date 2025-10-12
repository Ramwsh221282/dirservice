namespace DirectoryService.Contracts.Locations;

public sealed record LocationAddressDto(
    string Country,
    string Region,
    string City,
    string Building,
    IEnumerable<string> Additionals
);
