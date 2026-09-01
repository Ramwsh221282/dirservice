using System.Net;
using DirectoryService.Infrastructure.Identity.Jwt;
using Microsoft.Extensions.Primitives;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Middlewares;

public sealed class AuthenticationMiddleware
{
    public const string AuthenticatedUserKey = "AuthenticatedUser";

    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenValidator tokenValidator)
    {
        if (!RequiresAuthentication(context))
        {
            await _next(context);
            return;
        }

        Result<string> tokenResult = ExtractToken(context);
        if (tokenResult.IsFailure)
        {
            await WriteUnauthorized(context, tokenResult.Error);
            return;
        }

        Result<ValidatedToken> validationResult = tokenValidator.Validate(tokenResult.Value);
        if (validationResult.IsFailure)
        {
            await WriteUnauthorized(context, validationResult.Error);
            return;
        }

        context.Items[AuthenticatedUserKey] = validationResult.Value;
        await _next(context);
    }

    private static bool RequiresAuthentication(HttpContext context)
    {
        Endpoint? endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            return false;
        }

        if (endpoint.Metadata.GetMetadata<AllowAnonymousAccessAttribute>() != null)
        {
            return false;
        }

        return endpoint.Metadata.GetMetadata<RequiresAuthenticationAttribute>() != null;
    }

    private static Result<string> ExtractToken(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AuthorizationHeader, out StringValues headerValues))
        {
            return MissingTokenError();
        }

        if (headerValues.Count != 1)
        {
            return MissingTokenError();
        }

        string? header = headerValues[0];
        if (string.IsNullOrWhiteSpace(header))
        {
            return MissingTokenError();
        }

        if (!header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return MissingTokenError();
        }

        string token = header[BearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return MissingTokenError();
        }

        return token;
    }

    private static Error MissingTokenError()
    {
        return new Error(
            "Заголовок Authorization отсутствует или имеет неверный формат.",
            new UnauthorizedErrorType()
        );
    }

    private static async Task WriteUnauthorized(HttpContext context, Error error)
    {
        Result result = Result.Fail(error);
        EnvelopeTemplate template = EnvelopeTemplate.FromResult(result, context.Request.Path);

        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await context.Response.WriteAsJsonAsync(template);
    }
}
