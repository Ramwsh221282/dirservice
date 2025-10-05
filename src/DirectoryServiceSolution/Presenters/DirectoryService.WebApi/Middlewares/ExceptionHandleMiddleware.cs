using System.Net;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Middlewares;

public sealed class ExceptionHandleMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandleMiddleware> _logger;

    public ExceptionHandleMiddleware(
        ILogger<ExceptionHandleMiddleware> logger,
        RequestDelegate next
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            LogException(ex);
            await HandleExceptionalAction(context);
        }
    }

    private async Task HandleExceptionalAction(HttpContext context)
    {
        Error error = Error.ExceptionalError("Ошибка на стороне приложения.");
        Result result = Result.Fail(error);
        EnvelopeTemplate template = EnvelopeTemplate.FromResult(result, context.Request.Path);

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsJsonAsync(template);
    }

    private void LogException(Exception ex)
    {
        _logger.LogError("Exception: {Ex}", ex);
    }
}
