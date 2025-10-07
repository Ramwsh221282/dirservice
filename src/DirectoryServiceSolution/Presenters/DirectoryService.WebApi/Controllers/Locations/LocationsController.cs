using DirectoryService.Contracts;
using DirectoryService.UseCases.Locations.CreateLocation;
using DirectoryService.WebApi.Filters;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Controllers.Locations;

[ApiController]
[Route("api/locations")]
[TypeFilter<EndpointLoggingFilter>]
public sealed class LocationsController : ControllerBase
{
    [HttpPost("")]
    public async Task<IResult> CreateLocation(
        [FromBody] CreateLocationRequest request,
        [FromServices] CreateLocationCommandHandler handler,
        CancellationToken ct
    )
    {
        CreateLocationCommand command = new(request);
        Result<Guid> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(CreateLocation));
    }
}
