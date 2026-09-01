using System.Data;
using System.Text;
using Dapper;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.Snapshots;
using DirectoryService.UseCases.Locations.Contracts;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Locations;

public sealed class LocationsRepository : ILocationsRepository
{
    private const string SelectColumns = """
        SELECT
            id,
            address,
            name,
            time_zone,
            created_at,
            updated_at,
            deleted_at
        FROM locations
        """;

    private readonly IDbConnection _connection;
    private readonly Dictionary<Guid, LocationSnapshot> _snapshots = [];

    public LocationsRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task AddLocation(Location location, CancellationToken ct = default)
    {
        const string sql =
            """
            INSERT INTO locations (id, address, name, time_zone, created_at, updated_at, deleted_at)
            VALUES (@Id, @Address::jsonb, @Name, @TimeZone, @CreatedAt, @UpdatedAt, @DeletedAt)
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                Id = location.Id.Value,
                Address = LocationAddressSnapshot.FromAddress(location.Address).ToJson(),
                Name = location.Name.Value,
                TimeZone = location.TimeZone.Value,
                location.LifeCycle.CreatedAt,
                location.LifeCycle.UpdatedAt,
                location.LifeCycle.DeletedAt,
            },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
        _snapshots[location.Id.Value] = LocationSnapshot.FromLocation(location);
    }

    public async Task Update(Location location, CancellationToken ct = default)
    {
        Guid id = location.Id.Value;

        if (!_snapshots.TryGetValue(id, out LocationSnapshot? tracked))
        {
            throw new InvalidOperationException(
                $"Локация с id {id} не отслеживается репозиторием. Обновление невозможно."
            );
        }

        LocationSnapshot current = LocationSnapshot.FromLocation(location);

        StringBuilder setClause = new();
        DynamicParameters parameters = new();
        parameters.Add("@Id", id);

        if (!tracked.Address.IsSameAs(current.Address))
        {
            AppendSetClause(setClause, "address = @Address::jsonb");
            parameters.Add("@Address", current.Address.ToJson());
        }

        if (!string.Equals(tracked.Name, current.Name, StringComparison.Ordinal))
        {
            AppendSetClause(setClause, "name = @Name");
            parameters.Add("@Name", current.Name);
        }

        if (!string.Equals(tracked.TimeZone, current.TimeZone, StringComparison.Ordinal))
        {
            AppendSetClause(setClause, "time_zone = @TimeZone");
            parameters.Add("@TimeZone", current.TimeZone);
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

        string sql = $"UPDATE locations SET {setClause} WHERE id = @Id";
        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        await _connection.ExecuteAsync(command);

        _snapshots[id] = current;
    }

    public async Task<IEnumerable<Location>> GetBySet(
        LocationsIdSet set,
        CancellationToken ct = default
    )
    {
        Guid[] ids = [.. set.Ids.Select(i => i.Value)];

        string sql = $"""
            {SelectColumns}
            WHERE id = ANY(@Ids) AND deleted_at IS NULL
            """;

        CommandDefinition command = new(sql, new { Ids = ids }, cancellationToken: ct);
        IEnumerable<LocationRow> rows = await _connection.QueryAsync<LocationRow>(command);

        return [.. rows.Select(Track)];
    }

    public async Task<Result<Location>> GetById(Guid id, CancellationToken ct = default)
    {
        LocationId locationId = new(id);
        return await GetById(locationId, ct);
    }

    private async Task<Result<Location>> GetById(LocationId id, CancellationToken ct = default)
    {
        string sql = $"""
            {SelectColumns}
            WHERE id = @Id AND deleted_at IS NULL
            """;

        CommandDefinition command = new(sql, new { Id = id.Value }, cancellationToken: ct);
        LocationRow? row = await _connection.QueryFirstOrDefaultAsync<LocationRow>(command);

        return row == null ? Error.NotFoundError("Локация не найдена") : Track(row);
    }

    public async Task<LocationNameUniquesness> IsLocationNameUnique(
        LocationName name,
        CancellationToken ct = default
    )
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1 FROM locations
                WHERE name = @Name AND deleted_at IS NULL
            )
            """;

        CommandDefinition command = new(sql, new { Name = name.Value }, cancellationToken: ct);
        bool hasAny = await _connection.ExecuteScalarAsync<bool>(command);

        return hasAny
            ? new LocationNameUniquesness(false, name.Value)
            : new LocationNameUniquesness(true, string.Empty);
    }

    public async Task<IEnumerable<Location>> Get(
        LocationSpecification specification,
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

        if (!string.IsNullOrWhiteSpace(specification.TimeZone))
        {
            whereClause.Append(" AND time_zone = @TimeZone");
            parameters.Add("@TimeZone", specification.TimeZone);
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
        IEnumerable<LocationRow> rows = await _connection.QueryAsync<LocationRow>(command);

        return [.. rows.Select(Track)];
    }

    private Location Track(LocationRow row)
    {
        Location location = MapToLocation(row);
        _snapshots[location.Id.Value] = LocationSnapshot.FromLocation(location);
        return location;
    }

    private static void AppendSetClause(StringBuilder setClause, string clause)
    {
        if (setClause.Length > 0)
        {
            setClause.Append(", ");
        }

        setClause.Append(clause);
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
}
