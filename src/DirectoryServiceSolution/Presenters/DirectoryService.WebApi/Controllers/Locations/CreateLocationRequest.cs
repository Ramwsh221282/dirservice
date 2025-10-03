using DirectoryService.UseCases.Locations.CreateLocation;

namespace DirectoryService.WebApi.Controllers.Locations;

public sealed record CreateLocationRequest(
    string Name,
    IEnumerable<string> AddressParts,
    string TimeZone
)
{
    public CreateLocationCommand AsCommand()
    {
        return new CreateLocationCommand(Name, AddressParts, TimeZone);
    }
}
