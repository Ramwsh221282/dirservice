using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.Infrastructure.Identity.Commands.Logout;

public sealed record LogoutCommand : ICommand
{
    public string RefreshToken { get; }

    public LogoutCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}
