using DirectoryService.Core.DeparmentsContext;

namespace DirectoryService.Infrastructure.PostgreSQL.Snapshots;

public sealed record DepartmentSnapshot(
    Guid Id,
    string Identifier,
    string Name,
    string Path,
    short Depth,
    Guid? ParentId,
    int ChildrensCount,
    DepartmentAttachmentsSnapshot Attachments,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
)
{
    public static DepartmentSnapshot FromDepartment(Department department)
    {
        return new DepartmentSnapshot(
            department.Id.Value,
            department.Identifier.Value,
            department.Name.Value,
            department.Path.Value,
            department.Depth.Value,
            department.Parent?.Value,
            department.ChildrensCount.Value,
            DepartmentAttachmentsSnapshot.FromHistory(department.Attachments),
            department.LifeCycle.CreatedAt,
            department.LifeCycle.UpdatedAt,
            department.LifeCycle.DeletedAt
        );
    }
}
