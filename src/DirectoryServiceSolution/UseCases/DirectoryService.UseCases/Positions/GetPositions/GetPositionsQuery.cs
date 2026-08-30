using System.Data;
using System.Text;
using Dapper;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;

namespace DirectoryService.UseCases.Positions.GetPositions;

public sealed class GetPositionsQuery : IQuery<GetPositionsQueryResponse[]>
{
    public Guid? DepartmentId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public IEnumerable<SortOption>? SortOptions { get; set; }
}

public sealed class SortOption
{
    public string? Direction { get; set; }
    public string? Field { get; set; }
}

public sealed class GetPositionsQueryResponse
{
    public Guid DepartmentId { get; }
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
    public DateTime? DeletedAt { get; }

    public GetPositionsQueryResponse(
        Guid departmentId,
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        DepartmentId = departmentId;
        Id = id;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }
}

public sealed class GetPositionsQueryHandler : IQueryHandler<GetPositionsQuery, GetPositionsQueryResponse[]>
{
    private const int DEFAULT_PAGE = 1;
    private const int DEFAULT_PAGE_SIZE = 50;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetPositionsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetPositionsQueryResponse[]> Handle(GetPositionsQuery query, CancellationToken ct = default)
    {
        if (!query.DepartmentId.HasValue)
        {
            return [];
        }

        int page = query.Page ?? DEFAULT_PAGE;
        int pageSize = query.PageSize ?? DEFAULT_PAGE_SIZE;

        StringBuilder whereClause = new("WHERE 1=1");
        StringBuilder orderByClause = new();
        DynamicParameters parameters = new();

        if (query.DepartmentId.HasValue)
        {
            whereClause.Append(" AND dp.department_id = @DepartmentId");
            parameters.Add("@DepartmentId", query.DepartmentId.Value, DbType.Guid);
        }

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            whereClause.Append(" AND p.name ILIKE '%' || @Name || '%'");
            parameters.Add("@Name", query.Name, DbType.String);
        }

        if (!string.IsNullOrWhiteSpace(query.Description))
        {
            whereClause.Append(" AND p.description ILIKE '%' || @Description || '%'");
            parameters.Add("@Description", query.Description, DbType.String);
        }

        if (query.SortOptions != null)
        {
            foreach (SortOption option in query.SortOptions)
            {
                if (string.IsNullOrWhiteSpace(option.Field))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(option.Direction))
                {
                    continue;
                }

                string direction = option.Direction.Trim().ToUpper();
                if (!(direction == "ASC" || direction == "DESC"))
                {
                    continue;
                }

                string field = option.Field.Trim().ToLower();
                string? column = field switch
                {
                    "name" => "p.name",
                    "description" => "p.description",
                    "created" => "p.created_at",
                    "updated" => "p.updated_at",
                    "deleted" => "p.deleted_at",
                    _ => null,
                };

                if (column == null)
                {
                    continue;
                }

                if (orderByClause.Length > 0)
                {
                    orderByClause.Append(", ");
                }

                orderByClause.Append(string.Format("{0} {1}", column, direction));
            }
        }

        string orderByPart = orderByClause.Length == 0 ? string.Empty : "ORDER BY " + orderByClause;

        int limit = pageSize;
        int offset = (page - 1) * pageSize;
        parameters.Add("@Limit", limit, DbType.Int32);
        parameters.Add("@Offset", offset, DbType.Int32);

        string sql =
        """
        SELECT
            p.id as id,
            p.name as name,
            p.description as description,
            p.created_at as created_at,
            p.updated_at as updated_at,
            p.deleted_at as deleted_at,
            dp.department_id as department_id
        FROM positions p
        JOIN department_positions dp ON p.id = dp.position_id
        {0}
        {1}
        LIMIT @Limit OFFSET @Offset
        """;

        sql = string.Format(sql, whereClause, orderByPart);

        using IDbConnection connection = await _dbConnectionFactory.Create(ct);
        CommandDefinition commandDefinition = new(sql, parameters, cancellationToken: ct);
        IEnumerable<GetPositionsQueryResponse> data =
            await connection.QueryAsync<GetPositionsQueryResponse>(commandDefinition);

        return [.. data];
    }
}
