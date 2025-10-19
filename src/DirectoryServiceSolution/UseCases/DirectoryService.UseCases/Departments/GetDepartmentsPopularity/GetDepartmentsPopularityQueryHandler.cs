using System.Data;
using System.Text.Json;
using Dapper;
using DirectoryService.Contracts.Departments.GetDepartmentsPopularity;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;

namespace DirectoryService.UseCases.Departments.GetDepartmentsPopularity;

public sealed class GetDepartmentsPopularityQueryHandler
    : IQueryHandler<GetDepartmentsPopularityQuery, IEnumerable<GetDepartmentsPopularityResponse>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentsPopularityQueryHandler(IDbConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<IEnumerable<GetDepartmentsPopularityResponse>> Handle(
        GetDepartmentsPopularityQuery query,
        CancellationToken ct = default
    )
    {
        string sql = $"""
            WITH
                
            department_status AS (
                SELECT
                    dp.department_id,
                    (COUNT(dp.department_id)::float / NULLIF((SELECT COUNT(*) FROM positions), 0)) * 100 AS department_positions_percentage_popularity
                FROM department_positions dp
                GROUP BY dp.department_id
            ),
                
            department_location_stats AS (
                SELECT
                    dl.department_id,
                    (COUNT(dl.department_id)::float / NULLIF((SELECT COUNT(*) FROM locations), 0)) * 100 AS department_locations_percentage_popularity
                FROM department_locations dl
                GROUP BY dl.department_id
            )

            SELECT
                id,
                ROUND(COALESCE(department_status.department_positions_percentage_popularity, 0)::decimal, 2) as position_stats,
                ROUND(COALESCE(department_location_stats.department_locations_percentage_popularity, 0)::decimal, 2) as location_stats,    
                identifier, 
                name, 
                path, 
                depth, 
                parent_id, 
                childrens_count, 
                attachments, 
                created_at,    
                updated_at,
                
                COALESCE(
                        (SELECT                      
                                jsonb_agg(
                                    jsonb_build_object(                            
                                        'id', p.id,
                                        'name', p.name,
                                        'description', p.description,
                                        'created_at', p.created_at,                        
                                        'updated_at', p.updated_at
                                        )
                            ) as department_position             
                FROM department_positions dp
                LEFT JOIN positions p ON dp.position_id = p.id   
                WHERE dp.department_id = departments.id
                AND p.deleted_at IS null), '[]'::jsonb
                ) as department_positions,
                
                COALESCE(
                        (SELECT                      
                                jsonb_agg(
                                    jsonb_build_object(
                                        'id', l.id,
                                        'name', l.name,
                                        'time_zone', l.time_zone,
                                        'created_at', l.created_at,                        
                                        'updated_at', l.updated_at,
                                        'addresses', jsonb_build_array(l.address -> 'FullPath')
                                        )
                        ) as department_location
                 FROM department_locations dl
                 LEFT JOIN locations l ON dl.location_id = l.id
                 WHERE dl.department_id = departments.id
                 AND l.deleted_at IS NULL), '[]'::jsonb
                ) as department_locations

            FROM departments
            LEFT JOIN department_status ON departments.id = department_status.department_id
            LEFT JOIN department_location_stats ON departments.id = department_location_stats.department_id
            WHERE departments.deleted_at IS NULL
            ORDER BY position_stats {query.OrderMode}, location_stats {query.OrderMode}
            LIMIT 10
            """;

        CommandDefinition command = new(sql, cancellationToken: ct);
        using IDbConnection connection = await _connectionFactory.Create(ct);

        IEnumerable<GetDepartmentsPopularityDataModel> data =
        [
            .. await connection.QueryAsync<GetDepartmentsPopularityDataModel>(command),
        ];

        IEnumerable<GetDepartmentsPopularityResponse> response = data.Select(d =>
            d.ToResponseModel()
        );

        return response;
    }

    private sealed class GetDepartmentsPopularityDataModel
    {
        public required Guid Id { get; init; }
        public required double PositionStats { get; init; }
        public required double LocationStats { get; init; }
        public required string Identifier { get; init; }
        public required string Name { get; init; }
        public required string Path { get; init; }
        public required int Depth { get; init; }
        public required Guid? ParentId { get; init; }
        public required int ChildrensCount { get; init; }
        public required string Attachments { get; init; } // json
        public required DateTime CreatedAt { get; init; }
        public required DateTime UpdatedAt { get; init; }
        public required string DepartmentPositions { get; init; } // json
        public required string DepartmentLocations { get; init; } // json

        public GetDepartmentsPopularityResponse ToResponseModel() =>
            new()
            {
                Id = Id,
                PositionsPercentage = PositionStats,
                LocationsPercentage = LocationStats,
                Identifier = Identifier,
                Name = Name,
                Path = Path,
                Depth = Depth,
                ParentId = ParentId,
                ChildrensCount = ChildrensCount,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                Attachments = ToAttachmentsDto(),
                Locations = ToLocationsDto(),
                Positions = ToPositionsDto(),
            };

        private IEnumerable<DepartmentsPopularityAttachmentsDto> ToAttachmentsDto()
        {
            using JsonDocument document = JsonDocument.Parse(Attachments);
            List<DepartmentsPopularityAttachmentsDto> dtos = [];
            var array = document.RootElement.GetProperty(nameof(Attachments));
            foreach (var entry in array.EnumerateArray())
            {
                Guid id = entry.GetProperty("Id").GetGuid();
                DateTime attachedAt = entry.GetProperty("AttachedAt").GetDateTime();

                DepartmentsPopularityAttachmentsDto dto = new()
                {
                    Id = id,
                    AttachedAt = attachedAt,
                };
                dtos.Add(dto);
            }

            return dtos;
        }

        private IEnumerable<DepartmentLocationEntryPopularityDto> ToLocationsDto()
        {
            using JsonDocument document = JsonDocument.Parse(DepartmentLocations);
            var array = document.RootElement;
            List<DepartmentLocationEntryPopularityDto> dtos = [];

            foreach (var entry in array.EnumerateArray())
            {
                Guid id = entry.GetProperty("id").GetGuid();
                string name = entry.GetProperty("name").GetString()!;
                string fullPath = string.Join(
                    ", ",
                    entry.GetProperty("addresses").EnumerateArray().Select(i => i.GetString()!)
                );
                string timeZone = entry.GetProperty("time_zone").GetString()!;
                DateTime createdAt = entry.GetProperty("created_at").GetDateTime();
                DateTime updatedAt = entry.GetProperty("updated_at").GetDateTime();

                DepartmentLocationEntryPopularityDto dto = new()
                {
                    Id = id,
                    Name = name,
                    TimeZone = timeZone,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt,
                    FullPath = fullPath,
                };

                dtos.Add(dto);
            }

            return dtos;
        }

        private IEnumerable<DepartmentPositionEntryPopularityDto> ToPositionsDto()
        {
            using JsonDocument document = JsonDocument.Parse(DepartmentPositions);
            var array = document.RootElement;
            List<DepartmentPositionEntryPopularityDto> dtos = [];

            foreach (var entry in array.EnumerateArray())
            {
                Guid id = entry.GetProperty("id").GetGuid();
                string name = entry.GetProperty("name").GetString()!;
                DateTime createdAt = entry.GetProperty("created_at").GetDateTime();
                DateTime updatedAt = entry.GetProperty("updated_at").GetDateTime();
                string description = entry.GetProperty("description").GetString()!;

                DepartmentPositionEntryPopularityDto dto = new()
                {
                    Id = id,
                    CreatedAt = createdAt,
                    UpdatedAt = updatedAt,
                    Description = description,
                    Name = name,
                };

                dtos.Add(dto);
            }

            return dtos;
        }
    }
}
