using System.Net;

namespace DirectoryService.WebApi.Middlewares;

public sealed class ExceptionHandleMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandleMiddleware(RequestDelegate next)
    {
        _next = next;
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
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }

    private void LogException(Exception ex)
    {
        string message = ex.Message;
        string? trace = ex.StackTrace;
        string? source = ex.Source;
        string template = """
            Exception.
            Message: {0}
            Source: {1}
            Trance: {2}
            """;
        string messageToLog = string.Format(template, message, source, trace);
        Console.WriteLine(messageToLog);
    }
}
