using System.Data.Common;
using Dapper;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.UseCases.Departments.Contracts;
using Microsoft.EntityFrameworkCore;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments;

public sealed class DepartmentsRepository : IDepartmentsRepository
{
    private readonly ServiceDbContext _dbContext;

    public DepartmentsRepository(ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// возвращает связанные локации, которые только один раз присоединялись к указанному по id подразделению.
    /// вернет пустой список, если локации присоединялись к другим подразделениим (т.е если куда-то еще присоединились)
    /// </summary>    
    public async Task<IReadOnlyList<DepartmentLocation>> GetSingleTimeAttachedDepartmentLocations(DepartmentId id, CancellationToken ct)
    {
        Guid rawDepartmentId = id.Value;
        return await _dbContext.DepartmentLocations.FromSqlInterpolated<DepartmentLocation>(@$"
                SELECT dl.* FROM (
                    WITH owned_department_locations AS (        
                        SELECT * FROM department_locations dl 
                        WHERE dl.department_id = {rawDepartmentId}
                    )
                    SELECT     
                        odl.location_id    
                    FROM 
                        owned_department_locations odl
                    JOIN LATERAL (
                            SELECT 
                                dl.department_id as department_id 
                            FROM department_locations dl
                            WHERE 
                            dl.location_id = odl.location_id
                            ) department_location_records ON TRUE
                    GROUP BY odl.location_id
                    HAVING COUNT(odl.location_id) = 1
                    ) owned_locations
                    JOIN department_locations dl ON dl.location_id = owned_locations.location_id")
                    .ToListAsync(cancellationToken: ct);
    }

    public Task<IReadOnlyList<DepartmentLocation>> GetSingleTimeAttachedDepartmentLocations(Department department, CancellationToken ct)
    {
        return GetSingleTimeAttachedDepartmentLocations(department.Id, ct);
    }

    /// <summary>
    /// возвращает связанные должности, которые только один раз присоединялись к указанному по id подразделению.
    /// вернет пустой список, если должности присоединялись к другим подразделениим (т.е если куда-то еще присоединились)
    /// </summary>    
    public async Task<IReadOnlyList<DepartmentPosition>> GetSingleTimeAttachedDepartmentPositions(DepartmentId id, CancellationToken ct)
    {
        Guid rawDepartmentId = id.Value;
        return await _dbContext.DepartmentPositions.FromSqlInterpolated<DepartmentPosition>(@$"
                SELECT dp.* FROM (
                    WITH owned_department_positions AS (        
                        SELECT * FROM department_positions dp 
                        WHERE dp.department_id = {rawDepartmentId}
                    )
                    SELECT     
                        odp.position_id    
                    FROM 
                        owned_department_positions odp
                    JOIN LATERAL (
                        SELECT 
                            dp.department_id as department_id 
                        FROM department_positions dp
                        WHERE dp.position_id = odp.position_id) 
                        department_position_records ON TRUE
                    GROUP BY odp.position_id
                    HAVING COUNT(odp.position_id) = 1) owned_positions
                    JOIN department_positions dp ON dp.position_id = owned_positions.position_id
            ")
            .ToListAsync(cancellationToken: ct);
    }

    public Task<IReadOnlyList<DepartmentPosition>> GetSingleTimeAttachedDepartmentPositions(Department department, CancellationToken ct)
    {
        return GetSingleTimeAttachedDepartmentPositions(department.Id, ct);
    }

    public void Attach(Department department)
    {
        _dbContext.Departments.Attach(department);
    }

    public async Task<Result<Department>> GetById(Guid id, bool useLock = false, CancellationToken ct = default)
    {
        Result<DepartmentId> departmentId = DepartmentId.Create(id);
        if (departmentId.IsFailure)
        {
            return departmentId.Error;
        }

        return await GetById(departmentId.Value, useLock, ct);
    }

    public async Task<Result<Department>> GetById(DepartmentId id, bool useLock = false, CancellationToken ct = default)
    {
        Department? department = await _dbContext
            .Departments.Include(d => d.Locations)
            .Include(d => d.Positions)
            .FirstOrDefaultAsync(
                d => d.Id == id && d.LifeCycle.DeletedAt == null,
                cancellationToken: ct
            );

        if (department is null)
        {
            return Error.NotFoundError($"Подразделения с ID - {id.Value} не существует.");
        }

        if (useLock)
        {
            await BlockDepartment(id.Value);
        }

        return department;
    }

    public async Task<IEnumerable<Department>> GetByIdArray(
        IEnumerable<DepartmentId> ids,
        CancellationToken ct = default
    )
    {
        return await _dbContext
            .Departments.Where(d => ids.Contains(d.Id) && d.LifeCycle.DeletedAt == null)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Department>> GetByIdArray(
        DepartmentsIdSet ids,
        CancellationToken ct = default
    )
    {
        return await GetByIdArray(ids.DepartmentIds, ct);
    }

    public async Task Add(Department department, CancellationToken ct = default)
    {
        await _dbContext.Departments.AddAsync(department, ct);
    }

    /// <summary>
    /// Получение "разрешения" на передвижение подразделения в другое подразделение путем сравнения путей.
    /// </summary>
    public async Task<DepartmentMovementApproval> GetMovementApproval(
        Department parent,
        Department child,
        CancellationToken ct = default
    )
    {
        const string sql = "SELECT @parentPath::ltree @> @childPath::ltree as is_ancestor_owning;";
        string parentPath = parent.Path.Value;
        string childPath = child.Path.Value;
        CommandDefinition command = new(sql, new { parentPath, childPath }, cancellationToken: ct);

        DbConnection connection = _dbContext.Database.GetDbConnection();
        bool isDepartmentOwning = await connection.QueryFirstAsync<bool>(command);

        return new DepartmentMovementApproval(parent, child, isDepartmentOwning);
    }

    public async Task<Result<DepartmentMovement>> GetDepartmentMovement(
        DepartmentId parentId,
        DepartmentId childId,
        CancellationToken ct = default
    )
    {
        await BlockDepartment(parentId.Value);
        await BlockDepartment(childId.Value);

        Department? ancestor = await _dbContext.Departments.FirstOrDefaultAsync(
            d => d.Id == parentId,
            cancellationToken: ct
        );
        if (ancestor == null)
        {
            string message = $"Не найдено новое подразделение для передвижения.";
            return Error.NotFoundError(message);
        }

        Department? descendant = await _dbContext.Departments.FirstOrDefaultAsync(
            d => d.Id == childId,
            ct
        );
        if (descendant == null)
        {
            string message = $"Не найдено дочернее подразделение для передвижения.";
            return Error.NotFoundError(message);
        }

        return new DepartmentMovement(ancestor, descendant);
    }

    /// <summary>
    /// Получение "Движения подразделения".
    /// Если не найдено хотя бы одно из подразделений - ошибка
    /// </summary>
    public async Task<Result<DepartmentMovement>> GetDepartmentMovement(
        Guid parentId,
        Guid childId,
        CancellationToken ct = default
    )
    {
        DepartmentId typedParentId = DepartmentId.Create(parentId);
        DepartmentId typedChildId = DepartmentId.Create(childId);
        return await GetDepartmentMovement(typedParentId, typedChildId, ct);
    }

    public async Task<Result<Department>> GetParentDeparmentByChildPath(
        DepartmentPath path,
        CancellationToken cancellationToken = default
    )
    {
        return await GetParentDeparmentByChildPath(path.Value, cancellationToken);
    }

    public async Task<Result<Department>> GetParentDeparmentByChildPath(
        Department department,
        CancellationToken ct = default
    )
    {
        return await GetParentDeparmentByChildPath(department.Path.Value, ct);
    }

    /// <summary>
    /// Получение родительского подразделения дочернего подразделения по пути дочернего подразделения
    /// </summary>
    public async Task<Result<Department>> GetParentDeparmentByChildPath(
        string childPath,
        CancellationToken ct = default
    )
    {
        Department? department = await _dbContext
            .Departments.FromSqlInterpolated(
                $@"
                SELECT
                    id,
                    identifier,
                    name,
                    path,
                    depth,
                    parent_id,
                    childrens_count,
                    created_at as ""LifeCycle_CreatedAt"",
                    deleted_at as ""LifeCycle_DeletedAt"",
                    updated_at as ""LifeCycle_UpdatedAt"",
                    attachments
                FROM departments
                WHERE path @> {childPath}::ltree AND deleted_at IS NULL
                ORDER BY depth DESC
                LIMIT 1 OFFSET 1"
            )
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (department == null)
        {
            return Error.NotFoundError("Не найдено родительское подразделение.");
        }

        return department;
    }    

    public async Task RefreshDepartmentPathsFromDelete(Department department, DepartmentPath copy, CancellationToken ct)
    {        
        string refreshedPathString = department.Path.Value;
        string oldPathString = copy.Value;
        Guid id = department.Id.Value;        

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
        UPDATE
            departments 
        SET 
            path = ({refreshedPathString}::text || '.' || (subpath(path, 1)::ltree)::text)::ltree 
        WHERE 
            path <@ {oldPathString}::ltree AND id != {id};", 
            ct);
    }

    /// <summary>
    /// Обовление путей у дочерних подразделений для подразделения, которое переносится в другое подразделение на основи копии предыдущего пути.
    /// </summary>
    public async Task RefreshDepartmentChildPaths(
        Department department,
        DepartmentPath oldPath,
        CancellationToken ct = default
    )
    {
        string newPath = department.Path.Value;
        string oldPathString = oldPath.Value;
        Guid departmentId = department.Id.Value;

        const string sql = """
            UPDATE departments
            SET path = @newPath::ltree || subpath(path::ltree, nlevel(@oldPath::ltree))
            WHERE path::ltree <@ @oldPath::ltree
            AND id != @departmentId
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                newPath,
                oldPath = oldPathString,
                departmentId,
            },
            cancellationToken: ct
        );

        DbConnection connection = _dbContext.Database.GetDbConnection();
        await connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Блокирование подразделения и его детей, чтобы безопасно выполнить перенос подразделения в другое подразделение.
    /// </summary>
    private async Task BlockDepartment(Guid id)
    {
        DbConnection connection = _dbContext.Database.GetDbConnection();
        const string sql = """
            WITH controlled_department AS (
                SELECT id, 
                       path, 
                       depth,
                       parent_id,
                       attachments FROM departments WHERE id = @id           
            )
            SELECT
                dependant_departments.id,
                dependant_departments.path,
                dependant_departments.depth,
                dependant_departments.parent_id,
                dependant_departments.attachments
            FROM departments 
                AS dependant_departments
            CROSS JOIN controlled_department
            WHERE dependant_departments.path <@ controlled_department.path::ltree
            FOR UPDATE;
            """;
        await connection.ExecuteAsync(sql, new { id });
    }
}
