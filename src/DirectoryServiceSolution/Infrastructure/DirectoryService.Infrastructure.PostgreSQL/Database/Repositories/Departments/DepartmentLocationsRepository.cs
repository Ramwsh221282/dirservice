using System.Data;
using System.Text;
using System.Text.Json;
using Dapper;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.LocationsContext;
using DirectoryService.Core.LocationsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.Snapshots;
using DirectoryService.UseCases.Departments.Contracts;

namespace DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Departments;

public sealed class DepartmentLocationsRepository : IDepartmentLocationsRepository
{
    private readonly IDbConnection _connection;

    public DepartmentLocationsRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task Add(DepartmentLocation departmentLocation, CancellationToken ct = default)
    {
        const string sql =
            """
            INSERT INTO department_locations (department_id, location_id)
            VALUES (@DepartmentId, @LocationId)
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                DepartmentId = departmentLocation.DepartmentId.Value,
                LocationId = departmentLocation.LocationId.Value,
            },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
    }

    public async Task DeleteByDepartmentId(Guid departmentId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM department_locations WHERE department_id = @DepartmentId";

        CommandDefinition command = new(
            sql,
            new { DepartmentId = departmentId },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
    }

    public async Task<IEnumerable<DepartmentLocation>> Get(
        DepartmentLocationSpecification specification,
        CancellationToken ct = default
    )
    {
        StringBuilder whereClause = new("WHERE 1=1");
        DynamicParameters parameters = new();

        if (specification.DepartmentId.HasValue)
        {
            whereClause.Append(" AND dl.department_id = @DepartmentId");
            parameters.Add("@DepartmentId", specification.DepartmentId.Value);
        }

        if (specification.LocationId.HasValue)
        {
            whereClause.Append(" AND dl.location_id = @LocationId");
            parameters.Add("@LocationId", specification.LocationId.Value);
        }

        string sql = $"""
            SELECT
                d.id as DepartmentId,
                d.identifier as DepartmentIdentifier,
                d.name as DepartmentName,
                d.path as DepartmentPath,
                d.depth as DepartmentDepth,
                d.parent_id as DepartmentParentId,
                d.childrens_count as DepartmentChildrensCount,
                d.attachments as DepartmentAttachments,
                d.created_at as DepartmentCreatedAt,
                d.updated_at as DepartmentUpdatedAt,
                d.deleted_at as DepartmentDeletedAt,
                l.id as LocationId,
                l.address as LocationAddress,
                l.name as LocationName,
                l.time_zone as LocationTimeZone,
                l.created_at as LocationCreatedAt,
                l.updated_at as LocationUpdatedAt,
                l.deleted_at as LocationDeletedAt
            FROM department_locations dl
            INNER JOIN departments d ON dl.department_id = d.id
            INNER JOIN locations l ON dl.location_id = l.id
            {whereClause}
            """;

        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        IEnumerable<DepartmentLocationRow> rows =
            await _connection.QueryAsync<DepartmentLocationRow>(command);

        return [.. rows.Select(MapToDepartmentLocation)];
    }

    private static DepartmentLocation MapToDepartmentLocation(DepartmentLocationRow row)
    {
        Department department = Department.Create(
            DepartmentId.Create(row.DepartmentId),
            row.DepartmentParentId,
            DepartmentIdentifier.Create(row.DepartmentIdentifier),
            DepartmentName.Create(row.DepartmentName),
            DepartmentPath.Create(row.DepartmentPath),
            DepartmentDepth.Create(row.DepartmentDepth),
            DepartmentAttachmentsSnapshot.FromJson(row.DepartmentAttachments).ToHistory(),
            DepartmentChildrensCount.Create(row.DepartmentChildrensCount),
            EntityLifeCycle.Create(
                row.DepartmentDeletedAt,
                row.DepartmentCreatedAt,
                row.DepartmentUpdatedAt
            )
        );

        Location location = Location.Create(
            LocationAddressSnapshot.FromJson(row.LocationAddress).ToAddress(),
            LocationName.Create(row.LocationName),
            LocationTimeZone.Create(row.LocationTimeZone),
            [],
            new LocationId(row.LocationId),
            EntityLifeCycle.Create(
                row.LocationDeletedAt,
                row.LocationCreatedAt,
                row.LocationUpdatedAt
            )
        );

        return new DepartmentLocation(department, location);
    }

    private sealed class DepartmentLocationRow
    {
        public Guid DepartmentId { get; init; }
        public string DepartmentIdentifier { get; init; } = string.Empty;
        public string DepartmentName { get; init; } = string.Empty;
        public string DepartmentPath { get; init; } = string.Empty;
        public short DepartmentDepth { get; init; }
        public Guid? DepartmentParentId { get; init; }
        public int DepartmentChildrensCount { get; init; }
        public string DepartmentAttachments { get; init; } = string.Empty;
        public DateTime DepartmentCreatedAt { get; init; }
        public DateTime DepartmentUpdatedAt { get; init; }
        public DateTime? DepartmentDeletedAt { get; init; }

        public Guid LocationId { get; init; }
        public string LocationAddress { get; init; } = string.Empty;
        public string LocationName { get; init; } = string.Empty;
        public string LocationTimeZone { get; init; } = string.Empty;
        public DateTime LocationCreatedAt { get; init; }
        public DateTime LocationUpdatedAt { get; init; }
        public DateTime? LocationDeletedAt { get; init; }
    }
}
