namespace DirectoryService.WebApi.Middlewares;

public static class MiddlewareExtensions
{
    public static void UseExceptionHandleMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ExceptionHandleMiddleware>();
    }

    public static void UseAuthenticationMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
