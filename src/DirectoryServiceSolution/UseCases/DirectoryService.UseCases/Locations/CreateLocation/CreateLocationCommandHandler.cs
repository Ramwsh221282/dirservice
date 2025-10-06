using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Locations.Contracts;
using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;
using Serilog;

namespace DirectoryService.UseCases.Locations.CreateLocation;

public sealed class CreateLocationCommandHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILocationsRepository _repository;
    private readonly ILogger _logger;

    public CreateLocationCommandHandler(
        ILocationsRepository repository,
        ILogger logger,
        IValidator<CreateLocationCommand> validator
    )
    {
        _repository = repository;
        _logger = logger.ForContext<CreateLocationCommandHandler>();
        _validator = validator;
    }

    public async Task<Result<Guid>> Handle(
        CreateLocationCommand command,
        CancellationToken ct = default
    )
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return validationResult.AsFailureResult<Guid>();

        LocationAddress address = LocationAddress.Create(command.AddressParts);
        LocationName name = LocationName.Create(command.Name);
        LocationTimeZone timeZone = LocationTimeZone.Create(command.TimeZone);
        Location location = new Location(address, name, timeZone);

        Result<Guid> result = await _repository.AddLocation(location, ct);
        if (result.IsFailure)
        {
            _logger.Error("Error: {Err}", result.Error.Message);
            return result.Error;
        }

        _logger.Information("Создана локация: {Id} - {Name}", result.Value, command.Name);
        return result.Value;
    }
}
