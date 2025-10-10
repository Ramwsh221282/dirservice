using System.Data.Common;
using Dapper;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments.Pocos;
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

    public void Attach(params Department[] department)
    {
        foreach (Department item in department)
            Attach(item);
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
    /// Получение "Движения подразделения".
    /// Если движение выполняется как передвинуть подразделение к его дочернему - ошибка
    /// Если не найдено хотя бы одно из подразделений - ошибка
    /// </summary>
    public async Task<Result<DepartmentMovement>> GetDepartmentMovement(
        Guid parentId,
        Guid childId,
        CancellationToken ct = default
    )
    {
        await BlockDepartmentForMovement(parentId);
        await BlockDepartmentForMovement(childId);

        const string sql = """
            WITH ancestor_department_info AS
                (SELECT 
                     id,
                     identifier,
                     name,
                     path,
                     "Depth",
                     "Parent",
                     childrens_count,
                     created_at,
                     deleted_at,
                     updated_at,
                     attachments
                 FROM departments 
                 WHERE id = @parentId),
            descendant_department_info AS
                (SELECT
                     id,
                     identifier,
                     name,
                     path,
                     "Depth",
                     "Parent",
                     childrens_count,
                     created_at,
                     deleted_at,
                     updated_at,
                     attachments 
                 FROM departments 
                 WHERE id = @childId)
            SELECT
                ancestor_department_info.path::ltree @> descendant_department_info.path::ltree as is_ancestor_owning,
                ancestor_department_info.id as ancestor_id,
                ancestor_department_info.identifier as ancestor_identifier,
                ancestor_department_info.name as ancestor_name,
                ancestor_department_info."Depth" as ancestor_depth,
                ancestor_department_info.path as ancestor_path,
                ancestor_department_info."Parent" as ancestor_parent_id,
                ancestor_department_info.childrens_count as ancestor_childrens_count,
                ancestor_department_info.created_at as ancestor_created_at,
                ancestor_department_info.deleted_at as ancestor_deleted_at,
                ancestor_department_info.updated_at as ancestor_updated_at,
                ancestor_department_info.attachments as ancestor_attachments,
                
                descendant_department_info.id as descendant_id,
                descendant_department_info.identifier as descendant_identifier,
                descendant_department_info.name as descendant_name,
                descendant_department_info."Depth" as descendant_depth,
                descendant_department_info.path as descendant_path,
                descendant_department_info."Parent" as descendant_parent_id,
                descendant_department_info.childrens_count as descendant_childrens_count,
                descendant_department_info.created_at as descendant_created_at,
                descendant_department_info.deleted_at as descendant_deleted_at,
                descendant_department_info.updated_at as descendant_updated_at,
                descendant_department_info.attachments as descendant_attachments
            FROM ancestor_department_info
                CROSS JOIN descendant_department_info;
            """;

        CommandDefinition command = new CommandDefinition(
            sql,
            new { parentId, childId },
            cancellationToken: ct
        );
        DbConnection connection = _dbContext.Database.GetDbConnection();

        DepartmentMovementPoco[] movement = (
            await connection.QueryAsync<DepartmentMovementPoco>(command)
        ).ToArray();

        if (movement.Length == 0)
            return Error.NotFoundError(
                $"Не удается найти информацию для смены подразделения {childId} к {parentId}"
            );

        if (!movement[0].is_ancestor_owning)
            return Error.ConflictError(
                $"Нельзя передвинуть подразделение {childId} в его дочернее {parentId}."
            );

        return movement[0].ToDomainObject();
    }

    public async Task<Result<Department>> GetParentDeparmentByChildPath(
        DepartmentPath path,
        CancellationToken cancellationToken = default
    )
    {
        return await GetParentDeparmentByChildPath(path.Value, cancellationToken);
    }

    /// <summary>
    /// Получение родительского подразделения дочернего подразделения по пути дочернего подразделения
    /// </summary>
    public async Task<Result<Department>> GetParentDeparmentByChildPath(
        string childPath,
        CancellationToken ct = default
    )
    {
        const string sql = """
            SELECT 
                id as Id, 
                "Parent" as ParentId,
                identifier as Identifier,
                name as Name,
                created_at as CreatedAt,
                deleted_at as DeletedAt,
                updated_at as UpdatedAt,
                attachments as Attachments,
                path as Path,
                "Depth" as Depth,
                childrens_count as ChildrensCount
            FROM departments
            WHERE path @> @childPath::ltree AND deleted_at IS NULL
            ORDER BY depth DESC
            LIMIT 1 OFFSET 1
            """;

        CommandDefinition command = new CommandDefinition(
            sql,
            new { childPath },
            cancellationToken: ct
        );

        DbConnection connection = _dbContext.Database.GetDbConnection();
        DepartmentPoco? poco = await connection.QueryFirstOrDefaultAsync<DepartmentPoco>(command);

        if (poco == null)
            return Error.NotFoundError("Не найдено родительское подразделение.");
        return poco.ToDepartment();
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
                       "Depth",
                       "Parent",
                       attachments FROM departments WHERE id = @id           
            )
            SELECT
                dependant_departments.id,
                dependant_departments.path,
                dependant_departments."Depth",
                dependant_departments."Parent",
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
