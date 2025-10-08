using DirectoryService.Contracts.Positions;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Positions.CreatePosition;
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
        CreatePositionCommand command = new CreatePositionCommand(request);
        Result<Guid> created = await handler.Handle(command, ct);
        return created.FromResult(nameof(CreatePositionCommand));
    }
}