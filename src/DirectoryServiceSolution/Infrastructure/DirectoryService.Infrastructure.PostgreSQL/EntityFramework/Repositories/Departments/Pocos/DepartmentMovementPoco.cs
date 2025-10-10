using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments.Pocos;

/// <summary>
/// Poco объект для маппинга при помощи Dapper и последующего создания DepartmentMovement объекта
/// </summary>
public sealed class DepartmentMovementPoco
{
    public bool is_ancestor_owning { get; set; }
    public Guid ancestor_id { get; set; }
    public Guid? ancestor_parent_id { get; set; }
    public string ancestor_identifier { get; set; } = string.Empty;
    public string ancestor_name { get; set; } = string.Empty;
    public short ancestor_depth { get; set; }
    public string ancestor_attachments { get; set; } = string.Empty;
    public string ancestor_path { get; set; } = string.Empty;
    public int ancestor_childrens_count { get; set; }
    public DateTime ancestor_created_at { get; set; }
    public DateTime? ancestor_deleted_at { get; set; }
    public DateTime ancestor_updated_at { get; set; }

    public Guid descendant_id { get; set; }
    public Guid? descendant_parent_id { get; set; }
    public string descendant_identifier { get; set; } = string.Empty;
    public string descendant_name { get; set; } = string.Empty;
    public short descendant_depth { get; set; }
    public string descendant_attachments { get; set; } = string.Empty;
    public string descendant_path { get; set; } = string.Empty;
    public int descendant_childrens_count { get; set; }
    public DateTime descendant_created_at { get; set; }
    public DateTime? descendant_deleted_at { get; set; }
    public DateTime descendant_updated_at { get; set; }

    public DepartmentMovement ToDomainObject()
    {
        Department movingTo = ToDepartment(
            ancestor_id,
            ancestor_parent_id,
            ancestor_identifier,
            ancestor_name,
            ancestor_path,
            ancestor_depth,
            ancestor_attachments,
            ancestor_childrens_count,
            ancestor_created_at,
            ancestor_deleted_at,
            ancestor_updated_at
        );

        Department movable = ToDepartment(
            descendant_id,
            descendant_parent_id,
            descendant_identifier,
            descendant_name,
            descendant_path,
            descendant_depth,
            descendant_attachments,
            descendant_childrens_count,
            descendant_created_at,
            descendant_deleted_at,
            descendant_updated_at
        );

        return new DepartmentMovement(movingTo, movable);
    }

    private Department ToDepartment(
        Guid id,
        Guid? parentId,
        string identifier,
        string name,
        string path,
        short depth,
        string attachments,
        int childrensCount,
        DateTime created,
        DateTime? deleted,
        DateTime updated
    )
    {
        return Department.Create(
            DepartmentId.Create(id),
            parentId,
            DepartmentIdentifier.Create(identifier),
            DepartmentName.Create(name),
            DepartmentPath.Create(path),
            DepartmentDepth.Create(depth),
            DepartmentChildAttachmentsHistory.FromJson(attachments),
            DepartmentChildrensCount.Create(childrensCount),
            EntityLifeCycle.Create(deleted, created, updated)
        );
    }
}
