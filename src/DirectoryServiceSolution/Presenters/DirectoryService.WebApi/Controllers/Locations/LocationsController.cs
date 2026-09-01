using DirectoryService.WebApi.Middlewares;
using DirectoryService.Contracts.Locations.CreateLocation;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Locations.CreateLocation;
using DirectoryService.UseCases.Locations.GetLocation;
using DirectoryService.UseCases.Locations.GetLocations;
using DirectoryService.WebApi.Filters;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Controllers.Locations;

/// <summary>
/// Локации — адреса, к которым привязываются подразделения.
/// </summary>
/// <remarks>Все эндпоинты требуют access-токена.</remarks>
[ApiController]
[Route("api/locations")]
[RequiresAuthentication]
[TypeFilter<EndpointLoggingFilter>]
public sealed class LocationsController : ControllerBase
{
    /// <summary>
    /// Создаёт новую локацию.
    /// </summary>
    /// <remarks>
    /// Адрес задаётся списком уровней от крупного к мелкому:
    /// субъект → населённый пункт → улица → строение.
    ///
    /// Пример: `["Ленинградская область", "г. Всеволожск", "улица Ленина", "д. 1"]`.
    /// Для городов федерального значения область не указывается:
    /// `["г. Санкт-Петербург", "проспект Невский", "д. 25"]`.
    /// </remarks>
    /// <response code="200">Локация создана, в `value` — её идентификатор.</response>
    /// <response code="400">Некорректное название, часовой пояс или адрес.</response>
    /// <response code="401">Нет действительного access-токена.</response>
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

    /// <summary>
    /// Возвращает список локаций с фильтрацией, сортировкой и постраничной выдачей.
    /// </summary>
    /// <param name="page">Номер страницы, начиная с 1.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="nameSearch">Подстрока для поиска по названию.</param>
    /// <param name="isActive">Только активные (`true`) или только архивные (`false`).</param>
    /// <param name="departmentIds">Отбор локаций, привязанных к указанным подразделениям.</param>
    /// <param name="sortOptions">Поля сортировки, например `name`.</param>
    /// <param name="sortDirection">Направление: `asc` или `desc`.</param>
    /// <param name="handler">Внедряется контейнером.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <response code="200">Список локаций и общее количество.</response>
    /// <response code="401">Нет действительного access-токена.</response>
    [HttpGet("")]
    public async Task<IResult> GetLocations(
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "pageSize")] int? pageSize,
        [FromQuery(Name = "nameSearch")] string? nameSearch,
        [FromQuery(Name = "isActive")] bool? isActive,
        [FromQuery(Name = "departmentIds")] IEnumerable<Guid>? departmentIds,
        [FromQuery(Name = "sortOptions")] IEnumerable<string>? sortOptions,
        [FromQuery(Name = "sortDirection")] string? sortDirection,
        [FromServices] IQueryHandler<GetLocationsQuery, GetLocationsResponse> handler,
        CancellationToken ct
    )
    {
        GetLocationsQuery query = new(
            nameSearch,
            isActive,
            departmentIds,
            page,
            pageSize,
            sortOptions,
            sortDirection
        );
        GetLocationsResponse response = await handler.Handle(query, ct);
        return Results.Ok(response);
    }

    /// <summary>
    /// Возвращает одну локацию по идентификатору.
    /// </summary>
    /// <response code="200">Локация найдена.</response>
    /// <response code="401">Нет действительного access-токена.</response>
    /// <response code="404">Локация не найдена или архивирована.</response>
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetLocation(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetLocationQuery, GetLocationQueryResponse?> handler,
        CancellationToken ct
    )
    {
        GetLocationQuery query = new(id);
        GetLocationQueryResponse? response = await handler.Handle(query, ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }
}
