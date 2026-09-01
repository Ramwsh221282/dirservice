using DirectoryService.WebApi.Middlewares;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.Contracts.Departments.GetDepartmentsPopularity;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Departments.CreateDepartment;
using DirectoryService.UseCases.Departments.DeleteDepartment;
using DirectoryService.UseCases.Departments.GetDepartmentHierarchLazy;
using DirectoryService.UseCases.Departments.GetDepartmentsPopularity;
using DirectoryService.UseCases.Departments.GetHierarchicalDepartments.GetDepartmentsPrefetch;
using DirectoryService.UseCases.Departments.GetHierarchicalDepartments.GetDepartmentsPrefetchV2;
using DirectoryService.UseCases.Departments.UpdateDepartmentLocations;
using Microsoft.AspNetCore.Mvc;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.WebApi.Controllers.Departments;

/// <summary>
/// Подразделения и их иерархия.
/// </summary>
/// <remarks>
/// Иерархия хранится в PostgreSQL через тип `ltree`.
/// Все эндпоинты требуют access-токена.
///
/// Обратите внимание: простого `GET /api/departments` нет.
/// Для списка используйте `hierarchical`, `hierarchical-v2` или `with-popularity`.
/// </remarks>
[ApiController]
[Route("api/departments")]
[RequiresAuthentication]
public sealed class DepartmentsController : ControllerBase
{
    /// <summary>
    /// Создаёт подразделение.
    /// </summary>
    /// <remarks>
    /// Если указан `parentId`, подразделение становится дочерним и наследует путь родителя,
    /// а счётчик дочерних у родителя увеличивается. Идентификатор (`identifier`) должен быть
    /// уникальным и состоять из латинских букв, цифр и дефисов.
    /// Нужно указать хотя бы одну существующую локацию.
    /// </remarks>
    /// <response code="200">Подразделение создано, в `value` — его идентификатор.</response>
    /// <response code="400">Некорректные данные или локации не найдены.</response>
    /// <response code="401">Нет действительного access-токена.</response>
    /// <response code="409">Подразделение с таким идентификатором уже существует.</response>
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

    /// <summary>
    /// Возвращает непосредственных потомков подразделения.
    /// </summary>
    /// <remarks>Подходит для ленивой подгрузки дерева при раскрытии узла в интерфейсе.</remarks>
    /// <response code="200">Список дочерних подразделений.</response>
    /// <response code="401">Нет действительного access-токена.</response>
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
        GetDepartmentHierarchyLazyQuery query = new(id);
        IEnumerable<LazyHierarchicalDepartmentDto> result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }

    /// <summary>
    /// Архивирует подразделение вместе со всеми потомками.
    /// </summary>
    /// <remarks>
    /// Удаление мягкое: записи помечаются `deleted_at` и перестают возвращаться в выборках.
    /// Всё поддерево архивируется одним SQL-запросом, счётчик дочерних у родителя обновляется.
    /// </remarks>
    /// <response code="200">Подразделение архивировано.</response>
    /// <response code="401">Нет действительного access-токена.</response>
    /// <response code="404">Подразделение не найдено или уже архивировано.</response>
    [HttpDelete("{id:guid}")]
    public async Task<IResult> RemoveDepartment(
        [FromRoute(Name = "id")] Guid id,
        [FromServices] ICommandHandler<Guid, DeleteDepartmentCommand> handler,
        CancellationToken ct
    )
    {
        DeleteDepartmentCommand command = new(id);
        Result<Guid> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(DeleteDepartmentCommand));
    }

    /// <summary>
    /// Полностью заменяет набор локаций подразделения.
    /// </summary>
    /// <remarks>
    /// Семантика именно замены, а не добавления: прежние привязки удаляются.
    /// Все переданные локации должны существовать и быть активными.
    /// </remarks>
    /// <response code="200">Локации обновлены.</response>
    /// <response code="400">Пустой список или локации не найдены.</response>
    /// <response code="401">Нет действительного access-токена.</response>
    /// <response code="404">Подразделение не найдено.</response>
    [HttpPut("{id:guid}/locations")]
    public async Task<IResult> ReplaceLocations(
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler,
        CancellationToken ct
    )
    {
        request = request with { DepartmentId = id };
        UpdateDepartmentLocationsCommand command = new(request);
        Result<Guid> result = await handler.Handle(command, ct);
        return result.FromResult(nameof(UpdateDepartmentLocationsCommand));
    }

    /// <summary>
    /// Возвращает верхние подразделения с предзагрузкой потомков (вариант v2).
    /// </summary>
    /// <remarks>Альтернативная реализация того же сценария, что и `hierarchical`, с другим SQL-запросом.</remarks>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="prefetch">Глубина предзагрузки дочерних уровней.</param>
    /// <param name="handler">Внедряется контейнером.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <response code="200">Иерархический список подразделений.</response>
    /// <response code="401">Нет действительного access-токена.</response>
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
        GetDepartmentsHierarchyPrefetchRequest request = new(page, pageSize, prefetch);
        GetDepartmentsPrefetchV2Query query = new(
            request.Page,
            request.PageSize,
            request.Prefetch
        );
        GetHierarchicalDepartmentsPrefetchResponse result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }

    /// <summary>
    /// Возвращает верхние подразделения с предзагрузкой потомков.
    /// </summary>
    /// <remarks>Основной эндпоинт для отрисовки дерева подразделений.</remarks>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="prefetch">Глубина предзагрузки дочерних уровней.</param>
    /// <param name="handler">Внедряется контейнером.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <response code="200">Иерархический список подразделений.</response>
    /// <response code="401">Нет действительного access-токена.</response>
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
        GetDepartmentsHierarchyPrefetchRequest request = new(page, pageSize, prefetch);
        GetDepartmentsPrefetchQuery query = new(
            request.Page,
            request.PageSize,
            request.Prefetch
        );

        GetHierarchicalDepartmentsPrefetchResponse result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }

    /// <summary>
    /// Возвращает подразделения, отсортированные по количеству должностей.
    /// </summary>
    /// <param name="orderMode">Направление сортировки: `asc` или `desc`.</param>
    /// <param name="handler">Внедряется контейнером.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <response code="200">Список подразделений с показателем популярности.</response>
    /// <response code="401">Нет действительного access-токена.</response>
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
        GetDepartmentsPopularityRequest request = new(orderMode);
        GetDepartmentsPopularityQuery query = new(request.OrderMode);
        IEnumerable<GetDepartmentsPopularityResponse> result = await handler.Handle(query, ct);
        return Results.Ok(result);
    }
}
