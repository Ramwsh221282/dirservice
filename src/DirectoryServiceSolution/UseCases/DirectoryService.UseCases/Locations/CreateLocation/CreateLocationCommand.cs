using DirectoryService.Contracts;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed record CreateLocationCommand
{
    public string Name { get; }
    public IEnumerable<string> AddressParts { get; }
    public string TimeZone { get; }

    public CreateLocationCommand(CreateLocationRequest request)
        : this(request.Name, request.AddressParts, request.TimeZone) { }

    public CreateLocationCommand(string name, IEnumerable<string> addressParts, string timeZone)
    {
        Name = name;
        AddressParts = addressParts;
        TimeZone = timeZone;
    }
}
