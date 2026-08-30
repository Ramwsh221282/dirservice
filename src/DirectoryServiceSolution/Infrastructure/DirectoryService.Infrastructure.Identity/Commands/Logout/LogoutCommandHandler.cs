using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Models;
using DirectoryService.Infrastructure.Identity.Repositories;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Commands.Logout;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly ISessionsRepository _sessionsRepository;
    private readonly IIdentityTransactionSource _transactionSource;
    private readonly IValidator<LogoutCommand> _validator;

    public LogoutCommandHandler(
        ISessionsRepository sessionsRepository,
        IIdentityTransactionSource transactionSource,
        IValidator<LogoutCommand> validator)
    {
        _sessionsRepository = sessionsRepository;
        _transactionSource = transactionSource;
        _validator = validator;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken ct = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.AsFailureResult();
        }

        await using IIdentityTransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);

        SessionSpecification specification = SessionSpecification.Empty().WithRefreshToken(command.RefreshToken);

        Session? session = await _sessionsRepository.Get(specification, ct, transaction);
        if (session == null)
        {
            return Error.NotFoundError("Сессия не найдена.");
        }

        Result deleteResult = await _sessionsRepository.Delete(session, ct, transaction);
        if (deleteResult.IsFailure)
        {
            return deleteResult.Error;
        }

        Result commitResult = await transaction.Commit(ct);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return Result.Success();
    }
}
