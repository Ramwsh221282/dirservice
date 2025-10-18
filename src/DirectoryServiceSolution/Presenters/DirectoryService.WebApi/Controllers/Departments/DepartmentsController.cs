using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetDepartmentsPopularity;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.CreateDepartment;
using DirectoryService.UseCases.Departments.GetDepartmentsPopularity;
using DirectoryService.UseCases.Departments.UpdateDepartmentLocations;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Controllers.Departments;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreateDepartmentRequest request,
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        CancellationToken ct
    )
    {
        CreateDepartmentCommand command = new(request);
        Result<Guid> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(CreateDepartmentCommand));
    }

    [HttpPut("{id:guid}/locations")]
    public async Task<IResult> ReplaceLocations(
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromRoute] Guid id, // вопрос, нужен ли тут в request типе свойство DepartmentId?
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler,
        CancellationToken ct
    )
    {
        request = request with { DepartmentId = id };
        UpdateDepartmentLocationsCommand command = new(request);
        Result<Guid> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(UpdateDepartmentLocationsCommand));
    }

    [HttpPut("with-popularity")]
    public async Task<IResult> GetWithPopularity(
        [FromQuery(Name = "orderMode")] string? orderMode,
        IQueryHandler<
            GetDepartmentsPopularityQuery,
            IEnumerable<GetDepartmentsPopularityResponse>
        > handler,
        CancellationToken ct = default
    )
    {
        GetDepartmentsPopularityRequest request = new(orderMode);
        GetDepartmentsPopularityQuery query = new(request.OrderMode);
        IEnumerable<GetDepartmentsPopularityResponse> result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }
}
