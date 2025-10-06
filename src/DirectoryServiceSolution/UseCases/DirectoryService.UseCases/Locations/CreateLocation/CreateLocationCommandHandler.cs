using DirectoryService.Contracts;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Locations.Contracts;
using ResultLibrary;
using Serilog;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed class CreateLocationCommandHandler
{
    private readonly ILocationsRepository _repository;
    private readonly ILogger _logger;

    public CreateLocationCommandHandler(ILocationsRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger.ForContext<CreateLocationCommandHandler>();
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
            _logger.Error("Error: {Err}", errors.ErrorStrings());
            return errors.AsSingleError();
        }

        Location location = new Location(address, name, timeZone);
        Result<Guid> result = await _repository.AddLocation(location, ct);
        if (result.IsFailure)
        {
            _logger.Error("Error: {Err}", errors.ErrorStrings());
            return result.Error;
        }

        _logger.Information("Создана локация: {Id} - {Name}", result.Value, command.Name);

        return result.Value;
    }
}
