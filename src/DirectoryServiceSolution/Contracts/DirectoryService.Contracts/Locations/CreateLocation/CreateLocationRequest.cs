namespace DirectoryService.Contracts.Locations.CreateLocation;

public sealed record CreateLocationRequest(
    string Name,
    IEnumerable<string> AddressParts,
    string TimeZone
);
