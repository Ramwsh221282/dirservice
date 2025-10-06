namespace DirectoryService.Contracts;

public sealed record CreateLocationRequest(
    string Name,
    IEnumerable<string> AddressParts,
    string TimeZone
);
