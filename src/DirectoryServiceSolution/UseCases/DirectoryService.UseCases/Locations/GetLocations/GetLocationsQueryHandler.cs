using System.Data;
using System.Text.Json;
using Dapper;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;

namespace DirectoryService.UseCases.Locations.GetLocations;

public sealed class GetLocationsQueryHandler
    : IQueryHandler<GetLocationsQuery, GetLocationsResponse>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetLocationsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetLocationsResponse> Handle(
        GetLocationsQuery query,
        CancellationToken ct = default
    )
    {
        DynamicParameters parameters = new DynamicParameters();
        IQueryClause filterClause = _connectionFactory.CreateClause();
        IQueryClause orderingClause = _connectionFactory.CreateClause();
        IQueryClause departmentFilterClause = _connectionFactory.CreateClause();
        IQueryClause paginationClause = _connectionFactory.CreateClause();

        if (!string.IsNullOrWhiteSpace(query.NameSortMode))
            orderingClause = query.NameSortMode switch
            {
                _ when query.NameSortMode.Equals("ASC", StringComparison.OrdinalIgnoreCase) =>
                    orderingClause.AddClause("l.name ASC"),
                _ when query.NameSortMode.Equals("DESC", StringComparison.OrdinalIgnoreCase) =>
                    orderingClause.AddClause("l.name DESC"),
                _ => orderingClause,
            };

        if (!string.IsNullOrWhiteSpace(query.DateCreatedSortMode))
            orderingClause = query.DateCreatedSortMode switch
            {
                _ when query.DateCreatedSortMode.Equals(
                        "ASC",
                        StringComparison.OrdinalIgnoreCase
                    ) => orderingClause.AddClause("l.created_at ASC"),
                _ when query.DateCreatedSortMode.Equals(
                        "DESC",
                        StringComparison.OrdinalIgnoreCase
                    ) => orderingClause.AddClause("l.created_at DESC"),
                _ => orderingClause,
            };

        if (!string.IsNullOrWhiteSpace(query.NameSearch))
            filterClause = filterClause.AddClause(
                "l.name ILIKE '%' || @nameSearch || '%'",
                "nameSearch",
                query.NameSearch.Trim()
            );

        if (query.IsActive != null)
            filterClause = query.IsActive.Value switch
            {
                true => filterClause.AddClause("l.created_at IS NOT NULL"),
                false => filterClause.AddClause("l.created_at IS NULL"),
            };

        if (query.DepartmentIds != null && query.DepartmentIds.Any())
        {
            filterClause = orderingClause.AddClause(
                "departments_count != 0",
                "departmentIds",
                query.DepartmentIds.ToArray()
            );
            departmentFilterClause = orderingClause.AddClause("d.id = ANY @departmentIds");
        }

        int limit = query.PageSize;
        int offset = (query.Page - 1) * query.PageSize;
        paginationClause.AddClause("LIMIT @limit", "limit", limit, DbType.Int32);
        paginationClause.AddClause("OFFSET @offset", "offset", offset, DbType.Int32);

        string mailFilterSql = filterClause.FormSqlClause(" WHERE ", " AND ");
        string orderBySql = orderingClause.FormSqlClause(" ORDER BY ", ", ");
        string departmentFilterSql = departmentFilterClause.FormRawClause(
            " AND d.id = ANY @departmentIds "
        );
        string paginationAddition = paginationClause.FormSeperatedRawClause(" ");

        filterClause.InjectParameters(parameters);
        departmentFilterClause.InjectParameters(parameters);
        paginationClause.InjectParameters(parameters);

        string sql = $"""
            SELECT  
                l.id as id,        
                l.name as name,
                l.time_zone as time_zone,
                l.created_at as created_at,
                l.deleted_at as deleted_at,
                l.updated_at as updated_at,
                l.address as address_object,
                departments_data.departments_count as departments_count, 
                COALESCE(departments_data.department_object, '[]'::jsonb) as department_objects,
                COUNT(*) OVER() as total_count
            FROM locations as l
            LEFT JOIN LATERAL (
                SELECT 
                    COUNT(*) as departments_count,
                    jsonb_agg(
                       jsonb_build_object(
                                'id', d.id,
                                'name', d.name,
                                'path', d.path,
                                'identifier', d.identifier,
                                'childrensCount', d.childrens_count
                            )
                        ) as department_object
                FROM department_locations dl
                JOIN departments d ON dl.department_id = d.id
                WHERE dl.location_id = l.id {departmentFilterSql}           
                ) as departments_data ON true    
            {mailFilterSql}
            {orderBySql}
            {paginationAddition}
            """;

        using IDbConnection connection = await _connectionFactory.Create(ct);
        CommandDefinition commandDefinition = new(sql, parameters, cancellationToken: ct);

        IEnumerable<GetLocationsQueryData> data =
            await connection.QueryAsync<GetLocationsQueryData>(commandDefinition);

        GetLocationsResponse response = new GetLocationsResponse([], 0, query.Page, query.PageSize);
        if (data.Any())
            response = response with
            {
                TotalCount = data.First().TotalCount,
                Locations = data.Select(d => d.ToLocationView()),
            };

        return response;
    }

    private sealed class GetLocationsQueryData
    {
        private static readonly JsonSerializerOptions DepartmentSerializationOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string TimeZone { get; init; }
        public required DateTime CreatedAt { get; init; }
        public required DateTime? DeletedAt { get; init; }
        public required DateTime UpdatedAt { get; init; }
        public required string AddressObject { get; init; }
        public required string DepartmentObjects { get; init; }
        public required int DepartmentsCount { get; init; }

        public int TotalCount { get; init; }

        public LocationDto ToLocationView() =>
            new()
            {
                Id = Id,
                Name = Name,
                TimeZone = TimeZone,
                CreatedAt = CreatedAt,
                DeletedAt = DeletedAt,
                UpdatedAt = UpdatedAt,
                Address = ToAddressView(),
                Departments = ToDepartmentsView(),
                DepartmentsCount = DepartmentsCount,
            };

        private IEnumerable<LocationDepartmentDto> ToDepartmentsView()
        {
            IEnumerable<LocationDepartmentDto>? view = JsonSerializer.Deserialize<
                IEnumerable<LocationDepartmentDto>
            >(DepartmentObjects, DepartmentSerializationOptions);

            return view
                ?? throw new InvalidOperationException(
                    $"Invalid object: {DepartmentObjects} for mapping into: {nameof(LocationDepartmentDto)}"
                );
        }

        private LocationAddressDto ToAddressView()
        {
            LocationAddressDto? view = JsonSerializer.Deserialize<LocationAddressDto>(
                AddressObject
            );

            return view
                ?? throw new InvalidOperationException(
                    $"Invalid object: {AddressObject} for mapping into: {nameof(LocationAddressDto)}"
                );
        }
    }
}
