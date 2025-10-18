using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.Contracts.Departments.GetDepartmentsPopularity;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.CreateDepartment;
using DirectoryService.UseCases.Departments.GetDepartmentHierarchLazy;
using DirectoryService.UseCases.Departments.GetDepartmentsPopularity;
using DirectoryService.UseCases.Departments.GetHierarchicalDepartments.GetDepartmentsPrefetch;
using DirectoryService.UseCases.Departments.GetHierarchicalDepartments.GetDepartmentsPrefetchV2;
using DirectoryService.UseCases.Departments.UpdateDepartmentLocations;
using Microsoft.AspNetCore.Mvc;
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
        var command = new CreateDepartmentCommand(request);
        var result = await handler.Handle(command, ct);
        return result.FromResult(nameof(CreateDepartmentCommand));
    }

    [HttpGet("{id:guid}/hierarchy")]
    public async Task<IResult> GetHierarchy(
        [FromRoute] Guid id,
        IQueryHandler<
            GetDepartmentHierarchyLazyQuery,
            IEnumerable<LazyHierarchicalDepartmentDto>
        > handler,
        CancellationToken ct
    )
    {
        var query = new GetDepartmentHierarchyLazyQuery(id);
        var result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }

    [HttpPut("{id:guid}/locations")]
    public async Task<IResult> ReplaceLocations(
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler,
        CancellationToken ct
    )
    {
        request = request with { DepartmentId = id };
        var command = new UpdateDepartmentLocationsCommand(request);
        var result = await handler.Handle(command, ct);
        return result.FromResult(nameof(UpdateDepartmentLocationsCommand));
    }

    [HttpGet("hierarchical-v2")]
    public async Task<IResult> GetHierarchicalV2(
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "pageSize")] int? pageSize,
        [FromQuery(Name = "prefetch")] int? prefetch,
        IQueryHandler<
            GetDepartmentsPrefetchV2Query,
            GetHierarchicalDepartmentsPrefetchResponse
        > handler,
        CancellationToken ct
    )
    {
        var request = new GetDepartmentsHierarchyPrefetchRequest(page, pageSize, prefetch);
        var query = new GetDepartmentsPrefetchV2Query(
            request.Page,
            request.PageSize,
            request.Prefetch
        );
        var result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }

    [HttpGet("hierarchical")]
    public async Task<IResult> GetHierarchical(
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "pageSize")] int? pageSize,
        [FromQuery(Name = "prefetch")] int? prefetch,
        IQueryHandler<
            GetDepartmentsPrefetchQuery,
            GetHierarchicalDepartmentsPrefetchResponse
        > handler,
        CancellationToken ct
    )
    {
        var request = new GetDepartmentsHierarchyPrefetchRequest(page, pageSize, prefetch);
        var query = new GetDepartmentsPrefetchQuery(
            request.Page,
            request.PageSize,
            request.Prefetch
        );
        var result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }

    [HttpGet("with-popularity")]
    public async Task<IResult> GetWithPopularity(
        [FromQuery(Name = "orderMode")] string? orderMode,
        IQueryHandler<
            GetDepartmentsPopularityQuery,
            IEnumerable<GetDepartmentsPopularityResponse>
        > handler,
        CancellationToken ct = default
    )
    {
        var request = new GetDepartmentsPopularityRequest(orderMode);
        var query = new GetDepartmentsPopularityQuery(request.OrderMode);
        var result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }
}
