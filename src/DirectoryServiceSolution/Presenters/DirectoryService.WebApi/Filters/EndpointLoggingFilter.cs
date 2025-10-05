using Microsoft.AspNetCore.Mvc.Filters;

namespace DirectoryService.WebApi.Filters;

public sealed class EndpointLoggingFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        DateTime now = DateTime.UtcNow;
        Console.WriteLine(
            $"Endpoint {GetEndpointName(context)} execution started. Date: {now:dd.MM.yyyy HH.mm.ss}"
        );
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        DateTime now = DateTime.UtcNow;
        Console.WriteLine(
            $"Endpoint {GetEndpointName(context)} execution stopped. Date: {now:dd.MM.yyyy HH.mm.ss}"
        );
    }

    public string? GetEndpointName(FilterContext context)
    {
        Endpoint? endpoint = context.HttpContext.GetEndpoint();
        string? endpointName = endpoint?.DisplayName;
        return endpointName;
    }
}
