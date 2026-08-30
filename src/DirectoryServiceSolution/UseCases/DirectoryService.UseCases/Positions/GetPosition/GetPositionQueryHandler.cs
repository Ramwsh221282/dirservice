using System.Data;
using Dapper;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;

namespace DirectoryService.UseCases.Positions.GetPosition;

public sealed class GetPositionQueryHandler : IQueryHandler<GetPositionQuery, GetPositionQueryResponse?>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetPositionQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetPositionQueryResponse?> Handle(GetPositionQuery query, CancellationToken ct = default)
    {
        DynamicParameters parameters = new();
        parameters.Add("@Id", query.Id, DbType.Guid);

        string sql =
        """
        SELECT
            p.id as id,
            p.name as name,
            p.description as description,
            p.created_at as created_at,
            p.updated_at as updated_at,
            p.deleted_at as deleted_at,
            COALESCE(array_agg(dp.department_id) FILTER (WHERE dp.department_id IS NOT NULL), ARRAY[]::uuid[]) as department_ids
        FROM positions p
        LEFT JOIN department_positions dp ON p.id = dp.position_id
        WHERE p.id = @Id
        GROUP BY p.id, p.name, p.description, p.created_at, p.updated_at, p.deleted_at
        """;

        using IDbConnection connection = await _dbConnectionFactory.Create(ct);
        CommandDefinition commandDefinition = new(sql, parameters, cancellationToken: ct);
        GetPositionQueryResponse? result =
            await connection.QuerySingleOrDefaultAsync<GetPositionQueryResponse>(commandDefinition);

        return result;
    }
}
