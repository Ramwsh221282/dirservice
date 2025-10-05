using Microsoft.AspNetCore.Mvc.Filters;

namespace DirectoryService.WebApi.Filters;

public sealed class EndpointLoggingFilter : IActionFilter
{
    private readonly Serilog.ILogger _logger;

    public EndpointLoggingFilter(Serilog.ILogger logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.Information(
            "Эндпоинт {Endpoint} выполнение начато.",
            GetEndpointName(context)
        );
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.Information(
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
