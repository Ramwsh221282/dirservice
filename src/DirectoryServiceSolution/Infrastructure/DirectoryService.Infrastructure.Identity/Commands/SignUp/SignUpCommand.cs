using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.Infrastructure.Identity.Commands.SignUp;

public sealed record SignUpCommand : ICommand<Guid>
{
    public string Login { get; }
    public string Password { get; }

    public SignUpCommand(string login, string password)
    {
        Login = login;
        Password = password;
    }
}
