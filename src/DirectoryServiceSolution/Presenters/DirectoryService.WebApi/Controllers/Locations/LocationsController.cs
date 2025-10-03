using DirectoryService.UseCases.Locations.CreateLocation;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.WebApi.Controllers.Locations;

[ApiController]
[Route("api/locations")]
public sealed class LocationsController : ControllerBase
{
    [HttpPost("")]
    public async Task<IResult> CreateLocation(
        [FromBody] CreateLocationRequest request,
        [FromServices] CreateLocationCommandHandler handler,
        CancellationToken ct
    )
    {
        CreateLocationCommand command = request.AsCommand();
        Guid result = await handler.Handle(command, ct);
        return Results.Ok(result);
    }
}
