using System.Data;
using System.Text;
using Dapper;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.Snapshots;
using DirectoryService.UseCases.Positions.Contracts;

namespace DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Positions;

public sealed class PositionsRepository : IPositionsRepository
{
    private readonly IDbConnection _connection;
    private readonly Dictionary<Guid, PositionSnapshot> _snapshots = [];

    public PositionsRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task Add(Position position, CancellationToken ct = default)
    {
        const string sql =
            """
            INSERT INTO positions (id, name, description, created_at, updated_at, deleted_at)
            VALUES (@Id, @Name, @Description, @CreatedAt, @UpdatedAt, @DeletedAt)
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                Id = position.Id.Value,
                Name = position.Name.Value,
                Description = position.Description.Value,
                position.LifeCycle.CreatedAt,
                position.LifeCycle.UpdatedAt,
                position.LifeCycle.DeletedAt,
            },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
        _snapshots[position.Id.Value] = PositionSnapshot.FromPosition(position);
    }

    public async Task Update(Position position, CancellationToken ct = default)
    {
        Guid id = position.Id.Value;

        if (!_snapshots.TryGetValue(id, out PositionSnapshot? tracked))
        {
            throw new InvalidOperationException(
                $"Должность с id {id} не отслеживается репозиторием. Обновление невозможно."
            );
        }

        PositionSnapshot current = PositionSnapshot.FromPosition(position);

        StringBuilder setClause = new();
        DynamicParameters parameters = new();
        parameters.Add("@Id", id);

        if (!string.Equals(tracked.Name, current.Name, StringComparison.Ordinal))
        {
            AppendSetClause(setClause, "name = @Name");
            parameters.Add("@Name", current.Name);
        }

        if (!string.Equals(tracked.Description, current.Description, StringComparison.Ordinal))
        {
            AppendSetClause(setClause, "description = @Description");
            parameters.Add("@Description", current.Description);
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

        string sql = $"UPDATE positions SET {setClause} WHERE id = @Id";
        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        await _connection.ExecuteAsync(command);

        _snapshots[id] = current;
    }

    public async Task<PositionNameUniquesness> IsUnique(
        PositionName name,
        CancellationToken ct = default
    )
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1 FROM positions
                WHERE name = @Name AND deleted_at IS NULL
            )
            """;

        CommandDefinition command = new(sql, new { Name = name.Value }, cancellationToken: ct);
        bool hasAny = await _connection.ExecuteScalarAsync<bool>(command);

        return hasAny
            ? new PositionNameUniquesness(false, name.Value)
            : new PositionNameUniquesness(true, string.Empty);
    }

    public async Task<IEnumerable<Position>> Get(
        PositionSpecification specification,
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

        if (!string.IsNullOrWhiteSpace(specification.Description))
        {
            whereClause.Append(" AND description = @Description");
            parameters.Add("@Description", specification.Description);
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
            SELECT
                id,
                name,
                description,
                created_at,
                updated_at,
                deleted_at
            FROM positions
            {whereClause}
            """;

        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        IEnumerable<PositionRow> rows = await _connection.QueryAsync<PositionRow>(command);

        return [.. rows.Select(Track)];
    }

    private Position Track(PositionRow row)
    {
        Position position = MapToPosition(row);
        _snapshots[position.Id.Value] = PositionSnapshot.FromPosition(position);
        return position;
    }

    private static void AppendSetClause(StringBuilder setClause, string clause)
    {
        if (setClause.Length > 0)
        {
            setClause.Append(", ");
        }

        setClause.Append(clause);
    }

    private static Position MapToPosition(PositionRow row)
    {
        PositionId id = new(row.Id);
        PositionName name = PositionName.Create(row.Name);
        PositionDescription description = PositionDescription.Create(row.Description);
        EntityLifeCycle lifeCycle = EntityLifeCycle.Create(
            row.DeletedAt,
            row.CreatedAt,
            row.UpdatedAt
        );

        return Position.Create(id, name, description, lifeCycle, []);
    }

    private sealed class PositionRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public DateTime? DeletedAt { get; init; }
    }
}
