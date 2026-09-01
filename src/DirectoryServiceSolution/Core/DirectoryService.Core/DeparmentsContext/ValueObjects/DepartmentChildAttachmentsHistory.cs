namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentChildAttachmentsHistory
{
    private readonly List<DepartmentChildAttachment> _attachments;
    public IReadOnlyList<DepartmentChildAttachment> Attachments => _attachments;

    public DepartmentChildAttachmentsHistory()
    {
        _attachments = [];
    }

    public DepartmentChildAttachmentsHistory(IEnumerable<DepartmentChildAttachment> attachments)
    {
        _attachments = [.. attachments];
    }

    public DepartmentChildAttachmentsHistory Attach(DepartmentChildAttachment attachment)
    {
        return new DepartmentChildAttachmentsHistory([.. _attachments, attachment]);
    }

    public DepartmentChildAttachmentsHistory Detach(Department department)
    {
        return new DepartmentChildAttachmentsHistory(
            [.. _attachments.Where(a => a.Id != department.Id)]
        );
    }

    public int Count()
    {
        return _attachments.Count;
    }

    public bool IsAttached(DepartmentId departmentId)
    {
        return _attachments.Any(a => a.Id == departmentId);
    }

    public bool IsAttached(Department department)
    {
        return IsAttached(department.Id);
    }

    public static DepartmentChildAttachmentsHistory Empty()
    {
        return new();
    }
}
