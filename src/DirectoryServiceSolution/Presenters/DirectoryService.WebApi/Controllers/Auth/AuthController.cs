using DirectoryService.Contracts.Auth;
using DirectoryService.Infrastructure.Identity.Commands.Logout;
using DirectoryService.Infrastructure.Identity.Commands.RefreshToken;
using DirectoryService.Infrastructure.Identity.Commands.SignUp;
using DirectoryService.UseCases.Common.Cqrs;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;
using SignInCommand = DirectoryService.Infrastructure.Identity.Commands.SignIn.SignInCommand;
using SignInResult = DirectoryService.Infrastructure.Identity.Commands.SignIn.SignInResult;

namespace DirectoryService.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("sign-up")]
    public async Task<IResult> SignUp(
        [FromBody] SignUpRequest request,
        [FromServices] ICommandHandler<Guid, SignUpCommand> handler,
        CancellationToken ct
    )
    {
        SignUpCommand command = new(request.Login, request.Password);
        Result<Guid> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(SignUpCommand));
    }

    [HttpPost("sign-in")]
    public async Task<IResult> SignIn(
        [FromBody] SignInRequest request,
        [FromServices] ICommandHandler<SignInResult, SignInCommand> handler,
        CancellationToken ct
    )
    {
        SignInCommand command = new(request.Login, request.Password);
        Result<SignInResult> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(SignInCommand));
    }

    [HttpPost("refresh")]
    public async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        [FromServices] ICommandHandler<RefreshTokenResult, RefreshTokenCommand> handler,
        CancellationToken ct
    )
    {
        RefreshTokenCommand command = new(request.RefreshToken);
        Result<RefreshTokenResult> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(RefreshTokenCommand));
    }

    [HttpPost("logout")]
    public async Task<IResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] ICommandHandler<LogoutCommand> handler,
        CancellationToken ct
    )
    {
        LogoutCommand command = new(request.RefreshToken);
        Result result = await handler.Handle(command, ct);
        return result.FromResult(nameof(LogoutCommand));
    }
}
