using DirectoryService.Core.Common.ValueObjects;
using DirectoryService.Core.DeparmentsContext;
using DirectoryService.Core.DeparmentsContext.ValueObjects;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments.Pocos;

/// <summary>
/// Poco объект для маппинга из Dapper
/// </summary>
public class DepartmentPoco
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; }
    public DateTime? DeletedAt { get; }
    public DateTime UpdatedAt { get; }
    public string Attachments { get; set; } = string.Empty;
    public string Path { get; } = string.Empty;
    public short Depth { get; }
    public int ChildrensCount { get; }

    public Department ToDepartment()
    {
        return Department.Create(
            DepartmentId.Create(Id),
            ParentId,
            DepartmentIdentifier.Create(Identifier),
            DepartmentName.Create(Name),
            DepartmentPath.Create(Path),
            DepartmentDepth.Create(Depth),
            DepartmentChildAttachmentsHistory.FromJson(Attachments),
            DepartmentChildrensCount.Create(ChildrensCount),
            EntityLifeCycle.Create(DeletedAt, CreatedAt, UpdatedAt)
        );
    }
}
