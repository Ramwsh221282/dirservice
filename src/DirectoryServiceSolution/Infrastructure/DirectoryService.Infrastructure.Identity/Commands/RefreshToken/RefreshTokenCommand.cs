using DirectoryService.UseCases.Common.Cqrs;

namespace DirectoryService.Infrastructure.Identity.Commands.RefreshToken;

public sealed record RefreshTokenCommand : ICommand<RefreshTokenResult>
{
    public string RefreshToken { get; }

    public RefreshTokenCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}
