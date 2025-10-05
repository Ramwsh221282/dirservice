using Microsoft.AspNetCore.Mvc.Filters;

namespace DirectoryService.WebApi.Filters;

public sealed class EndpointLoggingFilter : IActionFilter
{
    private readonly ILogger<EndpointLoggingFilter> _logger;

    public EndpointLoggingFilter(ILogger<EndpointLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        DateTime now = DateTime.UtcNow;
        _logger.LogInformation(
            $"Эндпоинт {GetEndpointName(context)} выполнение начато. Дата: {now:dd.MM.yyyy HH.mm.ss}"
        );
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        DateTime now = DateTime.UtcNow;
        _logger.LogInformation(
            $"Эндпоинт {GetEndpointName(context)} выполнение остановлено. Дата: {now:dd.MM.yyyy HH.mm.ss}"
        );
    }

    public string? GetEndpointName(FilterContext context)
    {
        Endpoint? endpoint = context.HttpContext.GetEndpoint();
        string? endpointName = endpoint?.DisplayName;
        return endpointName;
    }
}
