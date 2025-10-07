using DirectoryService.Contracts;
using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed record CreateLocationCommand : ICommand<Guid>
{
    public string Name { get; }
    public IEnumerable<string> AddressParts { get; }
    public string TimeZone { get; }

    public CreateLocationCommand(CreateLocationRequest request)
        : this(
            request.Name.Name,
            request.AddressNodes.Select(n => n.Node),
            request.TimeZone.TimeZone
        ) { }

    public CreateLocationCommand(string name, IEnumerable<string> addressParts, string timeZone)
    {
        Name = name;
        AddressParts = addressParts;
        TimeZone = timeZone;
    }
}
