using System.Data.Common;
using Dapper;
using DirectoryService.Core.DeparmentsContext;
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

    public void Attach(Department department)
    {
        _dbContext.Departments.Attach(department);
    }

    public async Task<Result<Department>> GetById(Guid id, CancellationToken ct = default)
    {
        Result<DepartmentId> departmentId = DepartmentId.Create(id);
        return departmentId.IsFailure ? departmentId.Error : await GetById(departmentId, ct);
    }

    public async Task<Result<Department>> GetById(DepartmentId id, CancellationToken ct = default)
    {
        Department? department = await _dbContext
            .Departments.Include(d => d.Locations)
            .Include(d => d.Positions)
            .FirstOrDefaultAsync(
                d => d.Id == id && d.LifeCycle.DeletedAt == null,
                cancellationToken: ct
            );

        return department == null
            ? Error.NotFoundError($"Подразделения с ID - {id.Value} не существует.")
            : department;
    }

    public async Task<IEnumerable<Department>> GetByIdArray(
        IEnumerable<DepartmentId> ids,
        CancellationToken ct = default
    ) =>
        await _dbContext
            .Departments.Where(d => ids.Contains(d.Id) && d.LifeCycle.DeletedAt == null)
            .ToListAsync(ct);

    public async Task<IEnumerable<Department>> GetByIdArray(
        DepartmentsIdSet ids,
        CancellationToken ct = default
    ) => await GetByIdArray(ids.DepartmentIds, ct);

    public async Task Add(Department department, CancellationToken ct = default) =>
        await _dbContext.Departments.AddAsync(department, ct);

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
        await BlockDepartmentForMovement(parentId.Value);
        await BlockDepartmentForMovement(childId.Value);

        Department? ancestor = await _dbContext.Departments.FirstOrDefaultAsync(
            d => d.Id == parentId,
            cancellationToken: ct
        );
        if (ancestor == null)
            return Error.NotFoundError($"Не найдено новое подразделение для передвижения.");

        Department? descendant = await _dbContext.Departments.FirstOrDefaultAsync(
            d => d.Id == childId,
            ct
        );
        if (descendant == null)
            return Error.NotFoundError($"Не найдено дочернее подразделение для передвижения.");

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
    ) => await GetParentDeparmentByChildPath(path.Value, cancellationToken);

    public async Task<Result<Department>> GetParentDeparmentByChildPath(
        Department department,
        CancellationToken ct = default
    ) => await GetParentDeparmentByChildPath(department.Path.Value, ct);

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
            return Error.NotFoundError("Не найдено родительское подразделение.");
        return department;
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
    private async Task BlockDepartmentForMovement(Guid id)
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
