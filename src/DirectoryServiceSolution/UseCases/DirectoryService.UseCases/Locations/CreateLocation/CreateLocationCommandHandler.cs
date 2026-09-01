using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using DirectoryService.UseCases.Common.Transaction;
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
    private readonly ITransactionSource _transactionSource;
    private readonly ILogger _logger;

    public CreateLocationCommandHandler(
        ILocationsRepository repository,
        ITransactionSource transactionSource,
        ILogger logger,
        IValidator<CreateLocationCommand> validator
    )
    {
        _repository = repository;
        _transactionSource = transactionSource;
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
        {
            return validationResult.AsFailureResult<Guid>();
        }

        await using ITransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);

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

        Result committing = await transaction.CommitChanges(nameof(CreateLocationCommand), ct);
        if (committing.IsFailure)
        {
            return committing.Error;
        }

        _logger.Information("Создана локация: {Id} - {Name}", location.Value.Id.Value, command.Name);
        return location.Value.Id.Value;
    }
}
