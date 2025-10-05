using Microsoft.AspNetCore.Http;

namespace ResultLibrary.AspNetCore;

public sealed record EnvelopeHttpResult : IResult
{
    private readonly EnvelopeTemplate _template;

    public EnvelopeHttpResult(EnvelopeTemplate template)
    {
        _template = template;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _template.OperationStatus;
        await httpContext.Response.WriteAsJsonAsync(_template);
    }
}

public sealed record EnvelopeHttpResult<T> : IResult
{
    private readonly EnvelopeTemplate<T> _template;

    public EnvelopeHttpResult(EnvelopeTemplate<T> template)
    {
        _template = template;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _template.OperationStatus;
        await httpContext.Response.WriteAsJsonAsync(_template);
    }
}
