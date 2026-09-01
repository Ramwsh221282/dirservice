using System.Data;
using System.Text;
using Dapper;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.Snapshots;
using DirectoryService.UseCases.Departments.Contracts;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Departments;

public sealed class DepartmentsRepository : IDepartmentsRepository
{
    private const string SelectColumns = """
        SELECT
            id,
            identifier,
            name,
            path,
            depth,
            parent_id,
            childrens_count,
            attachments,
            created_at,
            updated_at,
            deleted_at
        FROM departments
        """;

    private readonly IDbConnection _connection;
    private readonly Dictionary<Guid, DepartmentSnapshot> _snapshots = [];

    public DepartmentsRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    private async Task DeleteSingleTimeAttachedDepartmentLocations(
        DepartmentId id,
        CancellationToken ct
    )
    {
        const string sql = """
            DELETE FROM
                department_locations
            WHERE
                location_id IN (
                SELECT odl.location_id
                FROM (
                    SELECT * FROM department_locations dl
                WHERE
                    dl.department_id = @DepartmentId
            ) odl
            GROUP BY odl.location_id
            HAVING COUNT(*) = 1)
            """;

        CommandDefinition command = new(
            sql,
            new { DepartmentId = id.Value },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
    }

    public Task DeleteSingleTimeAttachedDepartmentLocations(
        Department department,
        CancellationToken ct
    )
    {
        return DeleteSingleTimeAttachedDepartmentLocations(department.Id, ct);
    }

    private async Task DeleteSingleTimeAttachedDepartmentPositions(
        DepartmentId id,
        CancellationToken ct
    )
    {
        const string sql = """
            DELETE FROM
                department_positions
            WHERE
                position_id IN (
                SELECT odp.position_id
                FROM (
                    SELECT * FROM department_positions dp
                WHERE
                    dp.department_id = @DepartmentId
            ) odp
            GROUP BY odp.position_id
            HAVING COUNT(*) = 1)
            """;

        CommandDefinition command = new(
            sql,
            new { DepartmentId = id.Value },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
    }

    public Task DeleteSingleTimeAttachedDepartmentPositions(
        Department department,
        CancellationToken ct
    )
    {
        return DeleteSingleTimeAttachedDepartmentPositions(department.Id, ct);
    }

    public async Task<Result<Department>> GetById(
        Guid id,
        bool useLock = false,
        CancellationToken ct = default
    )
    {
        Result<DepartmentId> departmentId = DepartmentId.Create(id);
        if (departmentId.IsFailure)
        {
            return departmentId.Error;
        }

        return await GetById(departmentId.Value, useLock, ct);
    }

    public async Task<Result<Department>> GetById(
        DepartmentId id,
        bool useLock = false,
        CancellationToken ct = default
    )
    {
        string sql = $"""
            {SelectColumns}
            WHERE id = @Id AND deleted_at IS NULL
            """;

        CommandDefinition command = new(sql, new { Id = id.Value }, cancellationToken: ct);
        DepartmentRow? row = await _connection.QueryFirstOrDefaultAsync<DepartmentRow>(command);

        if (row is null)
        {
            return Error.NotFoundError($"Подразделения с ID - {id.Value} не существует.");
        }

        if (useLock)
        {
            await BlockDepartment(id.Value, ct);
        }

        return Track(row, await GetLocations(row.Id, ct));
    }

    private async Task<IEnumerable<Location>> GetLocations(Guid departmentId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                l.id,
                l.address,
                l.name,
                l.time_zone,
                l.created_at,
                l.updated_at,
                l.deleted_at
            FROM department_locations dl
            INNER JOIN locations l ON dl.location_id = l.id
            WHERE dl.department_id = @DepartmentId
            """;

        CommandDefinition command = new(
            sql,
            new { DepartmentId = departmentId },
            cancellationToken: ct
        );

        IEnumerable<LocationRow> rows = await _connection.QueryAsync<LocationRow>(command);
        return [.. rows.Select(MapToLocation)];
    }

    private static Location MapToLocation(LocationRow row)
    {
        LocationId id = new(row.Id);
        LocationAddress address = LocationAddressSnapshot.FromJson(row.Address).ToAddress();
        LocationName name = LocationName.Create(row.Name);
        LocationTimeZone timeZone = LocationTimeZone.Create(row.TimeZone);
        EntityLifeCycle lifeCycle = EntityLifeCycle.Create(
            row.DeletedAt,
            row.CreatedAt,
            row.UpdatedAt
        );

        return Location.Create(address, name, timeZone, [], id, lifeCycle);
    }

    private sealed class LocationRow
    {
        public Guid Id { get; init; }
        public string Address { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string TimeZone { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public DateTime? DeletedAt { get; init; }
    }

    private async Task<IEnumerable<Department>> GetByIdArray(
        IEnumerable<DepartmentId> ids,
        CancellationToken ct = default
    )
    {
        Guid[] rawIds = [.. ids.Select(i => i.Value)];

        string sql = $"""
            {SelectColumns}
            WHERE id = ANY(@Ids) AND deleted_at IS NULL
            """;

        CommandDefinition command = new(sql, new { Ids = rawIds }, cancellationToken: ct);
        IEnumerable<DepartmentRow> rows = await _connection.QueryAsync<DepartmentRow>(command);

        return [.. rows.Select(r => Track(r))];
    }

    public async Task<IEnumerable<Department>> GetByIdArray(
        DepartmentsIdSet ids,
        CancellationToken ct = default
    )
    {
        return await GetByIdArray(ids.DepartmentIds, ct);
    }

    public async Task<bool> HasWithPath(DepartmentPath path, CancellationToken ct = default)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1 FROM departments
                WHERE path = @Path::ltree AND deleted_at IS NULL
            )
            """;

        CommandDefinition command = new(sql, new { Path = path.Value }, cancellationToken: ct);
        return await _connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task Add(Department department, CancellationToken ct = default)
    {
        const string sql =
            """
            INSERT INTO departments (
                id, identifier, name, path, depth, parent_id,
                childrens_count, attachments, created_at, updated_at, deleted_at
            )
            VALUES (
                @Id, @Identifier, @Name, @Path::ltree, @Depth, @ParentId,
                @ChildrensCount, @Attachments::jsonb, @CreatedAt, @UpdatedAt, @DeletedAt
            )
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                Id = department.Id.Value,
                Identifier = department.Identifier.Value,
                Name = department.Name.Value,
                Path = department.Path.Value,
                Depth = department.Depth.Value,
                ParentId = department.Parent?.Value,
                ChildrensCount = department.ChildrensCount.Value,
                Attachments = DepartmentAttachmentsSnapshot
                    .FromHistory(department.Attachments)
                    .ToJson(),
                department.LifeCycle.CreatedAt,
                department.LifeCycle.UpdatedAt,
                department.LifeCycle.DeletedAt,
            },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
        _snapshots[department.Id.Value] = DepartmentSnapshot.FromDepartment(department);
    }

    public async Task Update(Department department, CancellationToken ct = default)
    {
        Guid id = department.Id.Value;

        if (!_snapshots.TryGetValue(id, out DepartmentSnapshot? tracked))
        {
            throw new InvalidOperationException(
                $"Подразделение с id {id} не отслеживается репозиторием. Обновление невозможно."
            );
        }

        DepartmentSnapshot current = DepartmentSnapshot.FromDepartment(department);

        StringBuilder setClause = new();
        DynamicParameters parameters = new();
        parameters.Add("@Id", id);

        if (!string.Equals(tracked.Identifier, current.Identifier, StringComparison.Ordinal))
        {
            AppendSetClause(setClause, "identifier = @Identifier");
            parameters.Add("@Identifier", current.Identifier);
        }

        if (!string.Equals(tracked.Name, current.Name, StringComparison.Ordinal))
        {
            AppendSetClause(setClause, "name = @Name");
            parameters.Add("@Name", current.Name);
        }

        if (!string.Equals(tracked.Path, current.Path, StringComparison.Ordinal))
        {
            AppendSetClause(setClause, "path = @Path::ltree");
            parameters.Add("@Path", current.Path);
        }

        if (tracked.Depth != current.Depth)
        {
            AppendSetClause(setClause, "depth = @Depth");
            parameters.Add("@Depth", current.Depth);
        }

        if (tracked.ParentId != current.ParentId)
        {
            AppendSetClause(setClause, "parent_id = @ParentId");
            parameters.Add("@ParentId", current.ParentId);
        }

        if (tracked.ChildrensCount != current.ChildrensCount)
        {
            AppendSetClause(setClause, "childrens_count = @ChildrensCount");
            parameters.Add("@ChildrensCount", current.ChildrensCount);
        }

        if (!tracked.Attachments.IsSameAs(current.Attachments))
        {
            AppendSetClause(setClause, "attachments = @Attachments::jsonb");
            parameters.Add("@Attachments", current.Attachments.ToJson());
        }

        if (tracked.UpdatedAt != current.UpdatedAt)
        {
            AppendSetClause(setClause, "updated_at = @UpdatedAt");
            parameters.Add("@UpdatedAt", current.UpdatedAt);
        }

        if (tracked.DeletedAt != current.DeletedAt)
        {
            AppendSetClause(setClause, "deleted_at = @DeletedAt");
            parameters.Add("@DeletedAt", current.DeletedAt);
        }

        if (setClause.Length == 0)
        {
            return;
        }

        string sql = $"UPDATE departments SET {setClause} WHERE id = @Id";
        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        await _connection.ExecuteAsync(command);

        _snapshots[id] = current;
    }

    public async Task<IEnumerable<Department>> Get(
        DepartmentSpecification specification,
        CancellationToken ct = default
    )
    {
        StringBuilder whereClause = new("WHERE 1=1");
        DynamicParameters parameters = new();

        if (specification.Id.HasValue)
        {
            whereClause.Append(" AND id = @Id");
            parameters.Add("@Id", specification.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(specification.Name))
        {
            whereClause.Append(" AND name = @Name");
            parameters.Add("@Name", specification.Name);
        }

        if (!string.IsNullOrWhiteSpace(specification.Identifier))
        {
            whereClause.Append(" AND identifier = @Identifier");
            parameters.Add("@Identifier", specification.Identifier);
        }

        if (!string.IsNullOrWhiteSpace(specification.Path))
        {
            whereClause.Append(" AND path = @Path::ltree");
            parameters.Add("@Path", specification.Path);
        }

        if (specification.Depth.HasValue)
        {
            whereClause.Append(" AND depth = @Depth");
            parameters.Add("@Depth", specification.Depth.Value);
        }

        if (specification.ParentId.HasValue)
        {
            whereClause.Append(" AND parent_id = @ParentId");
            parameters.Add("@ParentId", specification.ParentId.Value);
        }

        if (specification.IsDeleted.HasValue)
        {
            whereClause.Append(
                specification.IsDeleted.Value
                    ? " AND deleted_at IS NOT NULL"
                    : " AND deleted_at IS NULL"
            );
        }

        string sql = $"""
            {SelectColumns}
            {whereClause}
            """;

        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        IEnumerable<DepartmentRow> rows = await _connection.QueryAsync<DepartmentRow>(command);

        return [.. rows.Select(r => Track(r))];
    }

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

        bool isDepartmentOwning = await _connection.QueryFirstAsync<bool>(command);

        return new DepartmentMovementApproval(parent, child, isDepartmentOwning);
    }

    private async Task<Result<DepartmentMovement>> GetDepartmentMovement(
        DepartmentId parentId,
        DepartmentId childId,
        CancellationToken ct = default
    )
    {
        await BlockDepartment(parentId.Value, ct);
        await BlockDepartment(childId.Value, ct);

        string sql = $"""
            {SelectColumns}
            WHERE id = @Id
            """;

        CommandDefinition ancestorCommand = new(
            sql,
            new { Id = parentId.Value },
            cancellationToken: ct
        );
        DepartmentRow? ancestor = await _connection.QueryFirstOrDefaultAsync<DepartmentRow>(
            ancestorCommand
        );
        if (ancestor == null)
        {
            string message = "Не найдено новое подразделение для передвижения.";
            return Error.NotFoundError(message);
        }

        CommandDefinition descendantCommand = new(
            sql,
            new { Id = childId.Value },
            cancellationToken: ct
        );
        DepartmentRow? descendant = await _connection.QueryFirstOrDefaultAsync<DepartmentRow>(
            descendantCommand
        );
        if (descendant == null)
        {
            string message = "Не найдено дочернее подразделение для передвижения.";
            return Error.NotFoundError(message);
        }

        return new DepartmentMovement(Track(ancestor), Track(descendant));
    }

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

    private async Task<Result<Department>> GetParentDeparmentByChildPath(
        string childPath,
        CancellationToken ct = default
    )
    {
        string sql = $"""
            {SelectColumns}
            WHERE path @> @ChildPath::ltree AND deleted_at IS NULL
            ORDER BY depth DESC
            LIMIT 1 OFFSET 1
            """;

        CommandDefinition command = new(sql, new { ChildPath = childPath }, cancellationToken: ct);
        DepartmentRow? row = await _connection.QueryFirstOrDefaultAsync<DepartmentRow>(command);

        if (row == null)
        {
            return Error.NotFoundError("Не найдено родительское подразделение.");
        }

        return Track(row);
    }

    public async Task ArchiveChildDepartments(
        Department department,
        DepartmentPath oldPath,
        CancellationToken ct
    )
    {
        const string sql = """
            UPDATE departments
            SET path = @NewPath::ltree || subpath(path::ltree, nlevel(@OldPath::ltree)),
                childrens_count = 0,
                deleted_at = @DeletedAt,
                updated_at = @DeletedAt
            WHERE path::ltree <@ @OldPath::ltree
              AND id != @Id
              AND deleted_at IS NULL
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                NewPath = department.Path.Value,
                OldPath = oldPath.Value,
                Id = department.Id.Value,
                DeletedAt = DateTime.UtcNow,
            },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
    }

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

        await _connection.ExecuteAsync(command);
    }

    private async Task BlockDepartment(Guid id, CancellationToken ct = default)
    {
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

        CommandDefinition command = new(sql, new { id }, cancellationToken: ct);
        await _connection.ExecuteAsync(command);
    }

    private Department Track(DepartmentRow row, IEnumerable<Location>? locations = null)
    {
        Department department = MapToDepartment(row, locations);
        _snapshots[department.Id.Value] = DepartmentSnapshot.FromDepartment(department);
        return department;
    }

    private static void AppendSetClause(StringBuilder setClause, string clause)
    {
        if (setClause.Length > 0)
        {
            setClause.Append(", ");
        }

        setClause.Append(clause);
    }

    private static Department MapToDepartment(
        DepartmentRow row,
        IEnumerable<Location>? locations = null
    )
    {
        DepartmentId id = DepartmentId.Create(row.Id);
        DepartmentIdentifier identifier = DepartmentIdentifier.Create(row.Identifier);
        DepartmentName name = DepartmentName.Create(row.Name);
        DepartmentPath path = DepartmentPath.Create(row.Path);
        DepartmentDepth depth = DepartmentDepth.Create(row.Depth);
        DepartmentChildrensCount childrensCount = DepartmentChildrensCount.Create(
            row.ChildrensCount
        );
        DepartmentChildAttachmentsHistory attachments = DepartmentAttachmentsSnapshot
            .FromJson(row.Attachments)
            .ToHistory();
        EntityLifeCycle lifeCycle = EntityLifeCycle.Create(
            row.DeletedAt,
            row.CreatedAt,
            row.UpdatedAt
        );

        return Department.Create(
            id,
            row.ParentId,
            identifier,
            name,
            path,
            depth,
            attachments,
            childrensCount,
            lifeCycle,
            locations
        );
    }

    private sealed class DepartmentRow
    {
        public Guid Id { get; init; }
        public string Identifier { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public short Depth { get; init; }
        public Guid? ParentId { get; init; }
        public int ChildrensCount { get; init; }
        public string Attachments { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public DateTime? DeletedAt { get; init; }
    }
}
