using DirectoryService.WebApi.Middlewares;
using DirectoryService.Contracts.Positions;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Positions.CreatePosition;
using DirectoryService.UseCases.Positions.GetPosition;
using DirectoryService.UseCases.Positions.GetPositions;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Controllers.Positions;

/// <summary>
/// Должности и их привязка к подразделениям.
/// </summary>
/// <remarks>Все эндпоинты требуют access-токена.</remarks>
[ApiController]
[Route("api/positions")]
[RequiresAuthentication]
public class PositionsController
{
    /// <summary>
    /// Создаёт должность и привязывает её к подразделениям.
    /// </summary>
    /// <remarks>
    /// Название должно быть уникальным (до 100 символов), описание — до 1000.
    /// Нужно указать хотя бы одно существующее подразделение.
    /// </remarks>
    /// <response code="200">Должность создана, в `value` — её идентификатор.</response>
    /// <response code="400">Некорректное название, описание или список подразделений.</response>
    /// <response code="401">Нет действительного access-токена.</response>
    /// <response code="409">Должность с таким названием уже существует.</response>
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

    /// <summary>
    /// Возвращает должность по идентификатору.
    /// </summary>
    /// <response code="200">Должность найдена.</response>
    /// <response code="401">Нет действительного access-токена.</response>
    /// <response code="404">Должность не найдена.</response>
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

    /// <summary>
    /// Возвращает список должностей подразделения.
    /// </summary>
    /// <param name="departmentId">Идентификатор подразделения. Без него вернётся пустой список.</param>
    /// <param name="name">Подстрока для поиска по названию.</param>
    /// <param name="description">Подстрока для поиска по описанию.</param>
    /// <param name="page">Номер страницы, по умолчанию 1.</param>
    /// <param name="pageSize">Размер страницы, по умолчанию 50.</param>
    /// <param name="handler">Внедряется контейнером.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <response code="200">Список должностей.</response>
    /// <response code="401">Нет действительного access-токена.</response>
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
