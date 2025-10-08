using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Common.UnitOfWork;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateLocationCommandHandler(
        ILocationsRepository repository,
        ILogger logger,
        IUnitOfWork unitOfWork,
        IValidator<CreateLocationCommand> validator
    )
    {
        _repository = repository;
        _logger = logger.ForContext<CreateLocationCommandHandler>();
        _validator = validator;
        _unitOfWork = unitOfWork;
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
        LocationNameUniquesness uniquesness = await _repository.IsLocationNameUnique(name, ct);

        Result<Location> location = Location.CreateNew(name, address, timeZone, uniquesness);
        if (location.IsFailure)
        {
            _logger.Error("Error: {Err}", location.Error.Message);
            return location.Error;
        }
        
        await _repository.AddLocation(location, ct);
        Result result = await _unitOfWork.SaveChanges(ct);
        if (result.IsFailure)
        {
            _logger.Error("Error: {Err}", result.Error.Message);
            return result.Error;
        }

        _logger.Information("Создана локация: {Id} - {Name}", location.Value.Id.Value, command.Name);
        return location.Value.Id.Value;
    }
}
