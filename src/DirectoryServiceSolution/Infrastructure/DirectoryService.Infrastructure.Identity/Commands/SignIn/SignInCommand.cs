using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.Infrastructure.Identity.Commands.SignIn;

public sealed record SignInCommand : ICommand<SignInResult>
{
    public string Login { get; }
    public string Password { get; }

    public SignInCommand(string login, string password)
    {
        Login = login;
        Password = password;
    }
}
