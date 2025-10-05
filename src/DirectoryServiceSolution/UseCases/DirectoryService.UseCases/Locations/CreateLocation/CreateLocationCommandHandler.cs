using DirectoryService.Contracts;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Locations.Contracts;
using Microsoft.Extensions.Logging;
using ResultLibrary;
using Serilog;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed class CreateLocationCommandHandler
{
    private readonly ILocationsRepository _repository;
    private readonly ILogger<CreateLocationCommandHandler> _logger;

    public CreateLocationCommandHandler(
        ILocationsRepository repository,
        ILogger<CreateLocationCommandHandler> logger
    )
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateLocationCommand command,
        CancellationToken ct = default
    )
    {
        ErrorsCollection errors = [];
        IEnumerable<Result<LocationAddressPart>> addressParts = command.AddressParts.Select(
            LocationAddressPart.Create
        );
        errors.Add(addressParts);

        Result<LocationAddress> address = LocationAddress.Create(
            addressParts.Where(r => r.IsSuccess).Select(r => r.Value)
        );
        errors.Add(address);

        Result<LocationName> name = LocationName.Create(command.Name);
        errors.Add(name);

        Result<LocationTimeZone> timeZone = LocationTimeZone.Create(command.TimeZone);
        errors.Add(timeZone);

        if (errors.Contains())
        {
            _logger.LogError("");
            return errors.AsSingleError();
        }

        Location location = new Location(address, name, timeZone);
        Result<Guid> result = await _repository.AddLocation(location, ct);
        return result.IsFailure ? result.Error : result.Value;
    }
}
