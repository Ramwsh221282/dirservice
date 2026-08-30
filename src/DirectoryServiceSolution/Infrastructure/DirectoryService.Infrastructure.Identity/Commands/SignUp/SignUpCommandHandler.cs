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

namespace DirectoryService.Infrastructure.Identity.Commands.SignUp;

public sealed class SignUpCommandHandler : ICommandHandler<Guid, SignUpCommand>
{
    private readonly IIdentityRepository _repository;
    private readonly ISessionsRepository _sessionsRepository;
    private readonly IIdentityTransactionSource _transactionSource;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IValidator<SignUpCommand> _validator;

    public SignUpCommandHandler(
        IIdentityRepository repository,
        ISessionsRepository sessionsRepository,
        IIdentityTransactionSource transactionSource,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IValidator<SignUpCommand> validator)
    {
        _repository = repository;
        _sessionsRepository = sessionsRepository;
        _transactionSource = transactionSource;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _validator = validator;
    }

    public async Task<Result<Guid>> Handle(SignUpCommand command, CancellationToken ct = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.AsFailureResult<Guid>();
        }

        string login = command.Login.Trim();

        await using IIdentityTransactionScope transaction = await _transactionSource.ReceiveTransaction(ct);

        UserSpecification specification = UserSpecification.Empty().WithLogin(login);
        User? existingUser = await _repository.Get(specification, ct, transaction);
        if (existingUser != null)
        {
            return Error.ConflictError($"Пользователь с логином '{login}' уже существует.");
        }

        string hashedPassword = _passwordHasher.Hash(command.Password);
        User user = User.CreateNew(login, hashedPassword);

        Result addResult = await _repository.Add(user, ct, transaction);
        if (addResult.IsFailure)
        {
            return addResult.Error;
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

        return user.Id;
    }
}
