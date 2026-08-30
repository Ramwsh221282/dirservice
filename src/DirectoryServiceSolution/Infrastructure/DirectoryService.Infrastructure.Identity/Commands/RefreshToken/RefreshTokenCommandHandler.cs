using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Jwt;
using DirectoryService.Infrastructure.Identity.Models;
using DirectoryService.Infrastructure.Identity.Repositories;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenResult, RefreshTokenCommand>
{
    private readonly ISessionsRepository _sessionsRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IIdentityTransactionSource _transactionSource;
    private readonly IJwtProvider _jwtProvider;
    private readonly IValidator<RefreshTokenCommand> _validator;

    public RefreshTokenCommandHandler(
        ISessionsRepository sessionsRepository,
        IIdentityRepository identityRepository,
        IIdentityTransactionSource transactionSource,
        IJwtProvider jwtProvider,
        IValidator<RefreshTokenCommand> validator)
    {
        _sessionsRepository = sessionsRepository;
        _identityRepository = identityRepository;
        _transactionSource = transactionSource;
        _jwtProvider = jwtProvider;
        _validator = validator;
    }

    public async Task<Result<RefreshTokenResult>> Handle(
        RefreshTokenCommand command,
        CancellationToken ct = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.AsFailureResult<RefreshTokenResult>();
        }

        await using IIdentityTransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);

        SessionSpecification sessionSpecification = SessionSpecification
            .Empty()
            .WithRefreshToken(command.RefreshToken);
        Session? session = await _sessionsRepository.Get(sessionSpecification, ct, transaction);

        if (session == null)
        {
            return InvalidTokenError();
        }

        if (session.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            return Error.ValidationError("Срок действия refresh-токена истёк.");
        }

        UserSpecification userSpecification = UserSpecification.Empty().WithId(session.UserId);
        User? user = await _identityRepository.Get(userSpecification, ct, transaction);

        if (user == null || user.DeletedAt != null)
        {
            return InvalidTokenError();
        }

        GeneratedTokens generatedTokens = _jwtProvider.GenerateTokens(user.Id, user.Login);
        Session updatedSession = new(
            session.SessionId,
            session.UserId,
            generatedTokens.RefreshToken,
            generatedTokens.AccessToken,
            session.CreatedAt,
            generatedTokens.AccessTokenExpiresAt,
            generatedTokens.RefreshTokenExpiresAt
        );

        Result updateResult = await _sessionsRepository.Update(updatedSession, ct, transaction);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        Result commitResult = await transaction.Commit(ct);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return new RefreshTokenResult(
            updatedSession.AccessToken!,
            updatedSession.RefreshToken!,
            updatedSession.AccessTokenExpiresAt,
            updatedSession.RefreshTokenExpiresAt
        );
    }

    private static Error InvalidTokenError()
    {
        return Error.ValidationError("Недействительный refresh-токен.");
    }
}
