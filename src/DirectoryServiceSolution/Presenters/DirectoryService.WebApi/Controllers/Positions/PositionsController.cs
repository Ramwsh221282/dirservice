using DirectoryService.Contracts.Positions;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Positions.CreatePosition;
using DirectoryService.UseCases.Positions.GetPosition;
using DirectoryService.UseCases.Positions.GetPositions;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Controllers.Positions;

[ApiController]
[Route("api/positions")]
public class PositionsController
{
    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreatePositionRequest request,
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        CancellationToken ct)
    {
        CreatePositionCommand command = new(request);
        Result<Guid> created = await handler.Handle(command, ct);
        return created.FromResult(nameof(CreatePositionCommand));
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetPosition(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetPositionQuery, GetPositionQueryResponse?> handler,
        CancellationToken ct)
    {
        GetPositionQuery query = new(id);
        GetPositionQueryResponse? response = await handler.Handle(query, ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    [HttpGet]
    public async Task<IResult> GetPositions(
        [FromQuery(Name = "departmentId")] Guid? departmentId,
        [FromQuery(Name = "name")] string? name,
        [FromQuery(Name = "description")] string? description,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "pageSize")] int? pageSize,
        [FromServices] IQueryHandler<GetPositionsQuery, GetPositionsQueryResponse[]> handler,
        CancellationToken ct)
    {
        GetPositionsQuery query = new()
        {
            DepartmentId = departmentId,
            Name = name,
            Description = description,
            Page = page,
            PageSize = pageSize,
        };
        GetPositionsQueryResponse[] response = await handler.Handle(query, ct);
        return Results.Ok(response);
    }
}
