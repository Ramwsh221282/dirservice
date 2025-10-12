using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Locations.CreateLocation;
using DirectoryService.UseCases.Locations.GetLocations;
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

    [HttpGet("")]
    public async Task<IResult> GetLocations(
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "pageSize")] int? pageSize,
        [FromQuery(Name = "nameSort")] string? nameSort,
        [FromQuery(Name = "dateCreatedSort")] string? dateCreatedSort,
        [FromQuery(Name = "nameSearch")] string? nameSearch,
        [FromQuery(Name = "isActive")] bool? isActive,
        [FromQuery(Name = "departmentIds")] IEnumerable<Guid>? departmentIds,
        [FromServices] IQueryHandler<GetLocationsQuery, GetLocationsResponse> handler,
        CancellationToken ct
    )
    {
        GetLocationsQuery query = new(
            nameSort,
            dateCreatedSort,
            nameSearch,
            isActive,
            departmentIds,
            page,
            pageSize
        );

        GetLocationsResponse response = await handler.Handle(query, ct);
        return Results.Ok(response);
    }
}
