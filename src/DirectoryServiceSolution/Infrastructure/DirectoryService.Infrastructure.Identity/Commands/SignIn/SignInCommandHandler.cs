using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Hashing;
using DirectoryService.Infrastructure.Identity.Jwt;
using DirectoryService.Infrastructure.Identity.Models;
using DirectoryService.Infrastructure.Identity.Repositories;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Extensions;
using FluentValidation;
using FluentValidation.Results;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Commands.SignIn;

public sealed class SignInCommandHandler : ICommandHandler<SignInResult, SignInCommand>
{
    private readonly IIdentityRepository _repository;
    private readonly ISessionsRepository _sessionsRepository;
    private readonly IIdentityTransactionSource _transactionSource;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IValidator<SignInCommand> _validator;

    public SignInCommandHandler(
        IIdentityRepository repository,
        ISessionsRepository sessionsRepository,
        IIdentityTransactionSource transactionSource,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IValidator<SignInCommand> validator)
    {
        _repository = repository;
        _sessionsRepository = sessionsRepository;
        _transactionSource = transactionSource;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _validator = validator;
    }

    public async Task<Result<SignInResult>> Handle(SignInCommand command, CancellationToken ct = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return InvalidCredentialsError();
        }

        string login = command.Login.Trim();

        await using IIdentityTransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);

        UserSpecification specification = UserSpecification.Empty().WithLogin(login);
        User? user = await _repository.Get(specification, ct, transaction);

        if (user == null || user.DeletedAt != null)
        {
            return InvalidCredentialsError();
        }

        bool passwordMatches = _passwordHasher.Verify(command.Password, user.Password);
        if (!passwordMatches)
        {
            return InvalidCredentialsError();
        }

        GeneratedTokens generatedTokens = _jwtProvider.GenerateTokens(user.Id, user.Login);
        Session session = Session.CreateNew(
            user.Id,
            generatedTokens.RefreshToken,
            generatedTokens.AccessToken,
            generatedTokens.AccessTokenExpiresAt,
            generatedTokens.RefreshTokenExpiresAt
        );

        Result sessionAddResult = await _sessionsRepository.Add(session, ct, transaction);
        if (sessionAddResult.IsFailure)
        {
            return sessionAddResult.Error;
        }

        Result commitResult = await transaction.Commit(ct);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        SignInUserInfo userInfo = new(user.Id, user.Login, user.CreatedAt);
        SignInTokenInfo tokenInfo = new(
            session.AccessToken!,
            session.RefreshToken!,
            session.AccessTokenExpiresAt,
            session.RefreshTokenExpiresAt
        );

        return new SignInResult(userInfo, tokenInfo);
    }

    private static Error InvalidCredentialsError()
    {
        return Error.ValidationError("Неверный логин или пароль.");
    }
}
