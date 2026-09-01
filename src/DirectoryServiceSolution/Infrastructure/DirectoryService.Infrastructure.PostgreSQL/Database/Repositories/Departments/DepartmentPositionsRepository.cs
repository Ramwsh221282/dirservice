using System.Data;
using System.Text;
using Dapper;
using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.Entities;
using DirectoryService.Core.DeparmentsContext.ValueObjects;
using DirectoryService.Core.PositionsContext;
using DirectoryService.Core.PositionsContext.ValueObjects;
using DirectoryService.Infrastructure.PostgreSQL.Snapshots;
using DirectoryService.UseCases.Departments.Contracts;

namespace DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Departments;

public sealed class DepartmentPositionsRepository : IDepartmentPositionsRepository
{
    private readonly IDbConnection _connection;

    public DepartmentPositionsRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task Add(DepartmentPosition departmentPosition, CancellationToken ct = default)
    {
        const string sql =
            """
            INSERT INTO department_positions (department_id, position_id)
            VALUES (@DepartmentId, @PositionId)
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                DepartmentId = departmentPosition.DepartmentId.Value,
                PositionId = departmentPosition.PositionId.Value,
            },
            cancellationToken: ct
        );

        await _connection.ExecuteAsync(command);
    }

    public async Task<IEnumerable<DepartmentPosition>> Get(
        DepartmentPositionSpecification specification,
        CancellationToken ct = default
    )
    {
        StringBuilder whereClause = new("WHERE 1=1");
        DynamicParameters parameters = new();

        if (specification.DepartmentId.HasValue)
        {
            whereClause.Append(" AND dp.department_id = @DepartmentId");
            parameters.Add("@DepartmentId", specification.DepartmentId.Value);
        }

        if (specification.PositionId.HasValue)
        {
            whereClause.Append(" AND dp.position_id = @PositionId");
            parameters.Add("@PositionId", specification.PositionId.Value);
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
                p.id as PositionId,
                p.name as PositionName,
                p.description as PositionDescription,
                p.created_at as PositionCreatedAt,
                p.updated_at as PositionUpdatedAt,
                p.deleted_at as PositionDeletedAt
            FROM department_positions dp
            INNER JOIN departments d ON dp.department_id = d.id
            INNER JOIN positions p ON dp.position_id = p.id
            {whereClause}
            """;

        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        IEnumerable<DepartmentPositionRow> rows =
            await _connection.QueryAsync<DepartmentPositionRow>(command);

        return [.. rows.Select(MapToDepartmentPosition)];
    }

    private static DepartmentPosition MapToDepartmentPosition(DepartmentPositionRow row)
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

        Position position = Position.Create(
            new PositionId(row.PositionId),
            PositionName.Create(row.PositionName),
            PositionDescription.Create(row.PositionDescription),
            EntityLifeCycle.Create(
                row.PositionDeletedAt,
                row.PositionCreatedAt,
                row.PositionUpdatedAt
            ),
            []
        );

        return new DepartmentPosition(department, position);
    }

    private sealed class DepartmentPositionRow
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

        public Guid PositionId { get; init; }
        public string PositionName { get; init; } = string.Empty;
        public string PositionDescription { get; init; } = string.Empty;
        public DateTime PositionCreatedAt { get; init; }
        public DateTime PositionUpdatedAt { get; init; }
        public DateTime? PositionDeletedAt { get; init; }
    }
}
