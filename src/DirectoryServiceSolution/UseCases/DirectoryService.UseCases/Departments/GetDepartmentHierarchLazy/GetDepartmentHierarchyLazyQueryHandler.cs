using System.Data;
using Dapper;
using DirectoryService.Contracts.Departments.GetDepartmentsHierarchyPrefetch;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;
using DirectoryService.UseCases.Departments.GetHierarchicalDepartments.Common;

namespace DirectoryService.UseCases.Departments.GetDepartmentHierarchLazy;

public sealed class GetDepartmentHierarchyLazyQueryHandler
    : IQueryHandler<GetDepartmentHierarchyLazyQuery, IEnumerable<LazyHierarchicalDepartmentDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentHierarchyLazyQueryHandler(IDbConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<LazyHierarchicalDepartmentDto>> Handle(
        GetDepartmentHierarchyLazyQuery query,
        CancellationToken ct = default
    )
    {
        const string sql = """
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
                EXISTS(SELECT 1 FROM departments WHERE departments.parent_id = d.id) as has_more_children
            FROM departments d
            WHERE d.parent_id = @id
              AND d.deleted_at IS NULL
            """;

        var command = new CommandDefinition(sql, new { id = query.Id }, cancellationToken: ct);
        using IDbConnection connection = await _connectionFactory.Create(ct);
        return await connection.QueryAsync<LazyHierarchicalDepartmentDto>(command);
    }
}
