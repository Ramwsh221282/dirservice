using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Locations.Contracts;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed class CreateLocationCommandHandler
{
    private readonly ILocationsRepository _repository;

    public CreateLocationCommandHandler(ILocationsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateLocationCommand command, CancellationToken ct = default)
    {
        LocationAddress address = command.CreateAddress(command.CreateAddressParts());
        LocationName name = command.CreateLocationName();
        LocationTimeZone timeZone = command.CreateTimeZone();
        Location location = new Location(address, name, timeZone);
        await _repository.AddLocation(location, ct);
        return location.Id.Value;
    }
}
