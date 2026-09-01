using DirectoryService.Infrastructure.Identity.Commands.SignUp;
using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Models;
using DirectoryService.Infrastructure.Identity.Repositories;
using DirectoryService.Infrastructure.PostgreSQL.Seeding;
using DirectoryService.UseCases.Common.Cqrs;
using ResultLibrary;

namespace DirectoryService.WebApi.Seeding;

public sealed class UsersSeeder : ISeeder
{
    private static readonly (string Login, string Password)[] SeedUsers =
    [
        ("admin", "admin12345"),
        ("manager", "manager12345"),
        ("employee", "employee12345"),
    ];

    private readonly ICommandHandler<Guid, SignUpCommand> _signUpHandler;
    private readonly IIdentityRepository _identityRepository;
    private readonly Serilog.ILogger _logger;

    public UsersSeeder(
        ICommandHandler<Guid, SignUpCommand> signUpHandler,
        IIdentityRepository identityRepository,
        Serilog.ILogger logger)
    {
        _signUpHandler = signUpHandler;
        _identityRepository = identityRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.Information("Seeding users...");

        int created = 0;

        foreach ((string login, string password) in SeedUsers)
        {
            UserSpecification specification = UserSpecification.Empty().WithLogin(login);
            User? existing = await _identityRepository.Get(specification);

            if (existing != null)
            {
                _logger.Information("User {Login} already exists. Skipping.", login);
                continue;
            }

            SignUpCommand command = new(login, password);
            Result<Guid> result = await _signUpHandler.Handle(command);

            if (result.IsFailure)
            {
                _logger.Warning(
                    "Failed to seed user {Login}: {Error}",
                    login,
                    result.Error.Message
                );
                continue;
            }

            created += 1;
        }

        _logger.Information("Users seeding complete. Created {Count} users.", created);
    }
}
