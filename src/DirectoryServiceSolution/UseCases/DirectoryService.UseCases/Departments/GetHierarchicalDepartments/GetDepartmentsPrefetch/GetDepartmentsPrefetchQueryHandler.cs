using Dapper;
using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;
using DirectoryService.UseCases.Departments.GetHierarchicalDepartments.Common;

namespace DirectoryService.UseCases.Departments.GetHierarchicalDepartments.GetDepartmentsPrefetch;

public sealed class GetDepartmentsPrefetchQueryHandler
    : IQueryHandler<GetDepartmentsPrefetchQuery, GetHierarchicalDepartmentsPrefetchResponse>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentsPrefetchQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetHierarchicalDepartmentsPrefetchResponse> Handle(
        GetDepartmentsPrefetchQuery query,
        CancellationToken ct = default
    )
    {
        var sql = """
            WITH
                root_departments AS
                    (SELECT
                         d.id,
                         d.identifier,
                         d.name,
                         d.path,
                         d.depth,
                         d.parent_id,
                         d.childrens_count,
                         d.created_at,
                         d.updated_at,
                         COUNT(*) OVER() as roots_count
                     FROM departments d
                     WHERE d.deleted_at IS NULL AND
                         d.parent_id IS NULL
                     OFFSET @rootsOffset
                         LIMIT @rootsLimit),

                roots_count AS (SELECT COUNT(*) OVER() FROM departments where departments.parent_id IS NULL)

            SELECT DISTINCT
                roots_count.*,
                root_departments.*,
                (EXISTS(
                    SELECT 1
                    FROM departments
                    WHERE parent_id = root_departments.id
                    OFFSET @childsLimit
                        LIMIT 1)
                    ) as has_more_children
            FROM root_departments
                     CROSS JOIN roots_count

            UNION ALL

            SELECT
                        COUNT(*) OVER(),
                        childs.*,
                        (EXISTS(
                            SELECT 1
                            FROM departments
                            WHERE parent_id = childs.id)
                            ) as has_more_children
            FROM root_departments r
                     CROSS JOIN LATERAL (
                SELECT
                    d.id,
                    d.identifier,
                    d.name,
                    d.path,
                    d.depth,
                    d.parent_id,
                    d.childrens_count,
                    d.created_at,
                    d.updated_at,
                    COUNT(*) OVER() as roots_count
                FROM departments d
                WHERE d.deleted_at IS NULL
                  AND d.parent_id = r.id
                  AND d.deleted_at IS NULL
                LIMIT @childsLimit) childs
            """;

        var rootsOffset = (query.Page - 1) * query.PageSize;
        var rootsLimit = query.PageSize;
        var childsLimit = query.Prefetch;

        var command = new CommandDefinition(
            sql,
            new
            {
                rootsOffset,
                rootsLimit,
                childsLimit,
            },
            cancellationToken: ct
        );

        using var connection = await _connectionFactory.Create(ct);
        var data = await connection.QueryAsync<GetDepartmentsPrefetchDataModel>(command);
        return new HierarchicalDepartmentsMapper(data).Map();
    }
}
