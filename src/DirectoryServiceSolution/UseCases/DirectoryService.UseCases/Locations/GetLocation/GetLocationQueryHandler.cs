using System.Data;
using Dapper;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;

namespace DirectoryService.UseCases.Locations.GetLocation;

public sealed class GetLocationQueryHandler : IQueryHandler<GetLocationQuery, GetLocationQueryResponse?>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetLocationQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetLocationQueryResponse?> Handle(GetLocationQuery query, CancellationToken ct = default)
    {
        DynamicParameters parameters = new();
        parameters.Add("@Id", query.Id, DbType.Guid);

        string sql =
        """
        SELECT
            l.id as id,
            l.name as name,
            l.time_zone as time_zone,
            l.created_at as created_at,
            l.updated_at as updated_at,
            l.deleted_at as deleted_at,
            l.address as address_object,
            COALESCE(array_agg(dl.department_id) FILTER (WHERE dl.department_id IS NOT NULL), ARRAY[]::uuid[]) as department_ids
        FROM locations l
        LEFT JOIN department_locations dl ON l.id = dl.location_id
        WHERE l.id = @Id
        GROUP BY l.id, l.name, l.time_zone, l.created_at, l.updated_at, l.deleted_at, l.address
        """;

        using IDbConnection connection = await _dbConnectionFactory.Create(ct);
        CommandDefinition commandDefinition = new(sql, parameters, cancellationToken: ct);
        GetLocationQueryResponse? result =
            await connection.QuerySingleOrDefaultAsync<GetLocationQueryResponse>(commandDefinition);

        return result;
    }
}
