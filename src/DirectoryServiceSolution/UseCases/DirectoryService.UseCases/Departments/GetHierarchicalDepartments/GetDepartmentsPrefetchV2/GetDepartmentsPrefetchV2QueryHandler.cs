using System.Data;
using Dapper;
using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;
using DirectoryService.UseCases.Departments.GetHierarchicalDepartments.Common;

namespace DirectoryService.UseCases.Departments.GetHierarchicalDepartments.GetDepartmentsPrefetchV2;

public sealed class GetDepartmentsPrefetchV2QueryHandler
    : IQueryHandler<GetDepartmentsPrefetchV2Query, GetHierarchicalDepartmentsPrefetchResponse>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentsPrefetchV2QueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetHierarchicalDepartmentsPrefetchResponse> Handle(
        GetDepartmentsPrefetchV2Query query,
        CancellationToken ct = default
    )
    {
        const string sql = """
                           WITH root_departments AS
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
                                    EXISTS(SELECT 1 FROM departments c WHERE c.parent_id = d.id) as has_more_children,
                                    COUNT(*) OVER() as total_count
                                    FROM departments d
                                    WHERE d.parent_id IS NULL
                                    AND d.deleted_at IS NULL
                                    LIMIT @limit OFFSET @offset),
                               
                               ranked_children AS 
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
                                    ROW_NUMBER() OVER(PARTITION BY d.parent_id) as child_rank
                                    FROM departments d
                                    JOIN root_departments ON d.parent_id = root_departments.id)

                           SELECT roots.id,
                                  roots.identifier,
                                  roots.name,
                                  roots.path,
                                  roots.depth,
                                  roots.parent_id,
                                  roots.childrens_count,
                                  roots.created_at,
                                  roots.updated_at,
                                  roots.has_more_children,    
                                  roots.total_count as total_count
                           FROM root_departments roots

                           UNION ALL

                           SELECT
                               rk.id,
                               rk.identifier,
                               rk.name,
                               rk.path,
                               rk.depth,
                               rk.parent_id,
                               rk.childrens_count,
                               rk.created_at,
                               rk.updated_at,
                               EXISTS(SELECT 1 FROM departments d WHERE d.parent_id = rk.id) as has_more_children,
                               COUNT(*) OVER() as total_count
                               FROM ranked_children as rk
                           WHERE rk.child_rank <= @child_limit
                           """;

        int limit = query.PageSize;
        int offset = (query.Page - 1) * query.PageSize;
        int prefetch = query.Prefetch;

        var command = new CommandDefinition(
            sql,
            new
            {
                limit,
                offset,
                child_limit = prefetch,
            }
        );

        using IDbConnection connection = await _connectionFactory.Create(ct);
        var data = await connection.QueryAsync<HierarchicalDepartmentDataModel>(command);
        return new HierarchicalDepartmentsMapper(data).Map();
    }
}