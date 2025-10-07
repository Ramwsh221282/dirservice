using DirectoryService.Contracts.Departments;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.CreateDepartment;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Controllers.Departments;

[ApiController]
[Route("departments")]
public sealed class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateDepartmentRequest request, [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler, CancellationToken ct)
    {
        CreateDepartmentCommand command = new(request);
        Result<Guid> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(CreateDepartmentCommand));
    }
}