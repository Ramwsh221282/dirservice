using Microsoft.AspNetCore.Mvc.Filters;

namespace DirectoryService.WebApi.Filters;

public sealed class EndpointLoggingFilter : IActionFilter
{
    private readonly ILogger _logger;

    public EndpointLoggingFilter(ILogger logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation(
            "Эндпоинт {Endpoint} выполнение начато.",
            GetEndpointName(context)
        );
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation(
            "Эндпоинт {Endpoint} выполнение остановлено.",
            GetEndpointName(context)
        );
    }

    private string? GetEndpointName(FilterContext context)
    {
        Endpoint? endpoint = context.HttpContext.GetEndpoint();
        string? endpointName = endpoint?.DisplayName;
        return endpointName;
    }
}
