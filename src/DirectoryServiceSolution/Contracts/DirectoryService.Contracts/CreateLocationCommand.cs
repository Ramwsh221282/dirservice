namespace DirectoryService.Contracts;

public sealed record CreateLocationCommand(
    string Name,
    IEnumerable<string> AddressParts,
    string TimeZone
);
