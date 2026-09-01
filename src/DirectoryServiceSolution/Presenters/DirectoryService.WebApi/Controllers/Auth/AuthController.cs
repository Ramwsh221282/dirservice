using DirectoryService.Contracts.Auth;
using DirectoryService.Infrastructure.Identity.Commands.Logout;
using DirectoryService.Infrastructure.Identity.Commands.RefreshToken;
using DirectoryService.Infrastructure.Identity.Commands.SignUp;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.WebApi.Middlewares;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;
using SignInCommand = DirectoryService.Infrastructure.Identity.Commands.SignIn.SignInCommand;
using SignInResult = DirectoryService.Infrastructure.Identity.Commands.SignIn.SignInResult;

namespace DirectoryService.WebApi.Controllers.Auth;

/// <summary>
/// Регистрация, вход, обновление и отзыв токенов.
/// </summary>
/// <remarks>
/// Эндпоинты этого контроллера намеренно доступны без access-токена:
/// именно здесь токен и выдаётся. Все остальные контроллеры требуют авторизации.
/// </remarks>
[ApiController]
[Route("api/auth")]
[AllowAnonymousAccess]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    /// <remarks>
    /// Логин — минимум 3 символа, пароль — минимум 6.
    /// Логин должен быть уникальным, иначе вернётся 409.
    /// В ответе приходит идентификатор созданного пользователя.
    /// </remarks>
    /// <response code="200">Пользователь создан, в `value` — его идентификатор.</response>
    /// <response code="400">Логин или пароль не прошли валидацию.</response>
    /// <response code="409">Пользователь с таким логином уже существует.</response>
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

    /// <summary>
    /// Выполняет вход и выдаёт пару токенов.
    /// </summary>
    /// <remarks>
    /// В ответе `value.token.accessToken` (живёт 5 минут) и `value.token.refreshToken` (7 дней).
    /// Access-токен передавайте в заголовке `Authorization: Bearer &lt;токен&gt;`.
    /// </remarks>
    /// <response code="200">Вход выполнен, в `value` — данные пользователя и пара токенов.</response>
    /// <response code="400">Неверный логин или пароль.</response>
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

    /// <summary>
    /// Обменивает refresh-токен на новую пару токенов.
    /// </summary>
    /// <remarks>
    /// Вызывайте, когда access-токен истёк и защищённый эндпоинт вернул 401.
    /// Access-токен здесь не нужен — он к этому моменту уже недействителен.
    /// Ротация полная: старый refresh-токен перестаёт работать, сохраняйте новый.
    /// </remarks>
    /// <response code="200">Выдана новая пара токенов.</response>
    /// <response code="400">Refresh-токен пуст, недействителен или истёк.</response>
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

    /// <summary>
    /// Завершает сессию по refresh-токену.
    /// </summary>
    /// <remarks>
    /// Сессия удаляется из базы, refresh-токен становится недействительным.
    /// Ранее выданный access-токен остаётся валидным до истечения своих 5 минут —
    /// это обычный компромисс JWT: подпись проверяется без обращения к базе.
    /// </remarks>
    /// <response code="200">Сессия завершена.</response>
    /// <response code="400">Refresh-токен пуст или не найден.</response>
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
