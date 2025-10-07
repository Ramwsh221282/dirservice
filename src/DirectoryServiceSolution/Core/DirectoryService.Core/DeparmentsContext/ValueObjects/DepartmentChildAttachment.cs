using System.Text.Json;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public sealed record DepartmentAttachmentsHistory
{
    private readonly List<DepartmentChildAttachment> _attachments = [];

    public IReadOnlyList<DepartmentChildAttachment> Attachments => _attachments;

    public DepartmentAttachmentsHistory()
    {
      // ef core   
    }

    public int Count() => _attachments.Count;
    
    public DepartmentAttachmentsHistory(IEnumerable<DepartmentChildAttachment> attachments) =>
        _attachments = [..attachments];
    
    public bool IsAttached(DepartmentId departmentId) => _attachments.Any(a => a.Id == departmentId);
    
    public bool IsAttached(Department department) => IsAttached(department.Id);

    public static Result<DepartmentAttachmentsHistory> FromJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        List<DepartmentChildAttachment> attachments = [];
        
        foreach (JsonElement entry in document.RootElement.EnumerateArray())
        {
            Result<DepartmentChildAttachment> attachment = DepartmentChildAttachment.FromJson(entry);
            if (attachment.IsFailure)
                return attachment.Error;
            attachments.Add(attachment);
        }

        return new DepartmentAttachmentsHistory(attachments);
    }
}

public sealed record DepartmentChildAttachment(DepartmentId Id, DateTime AttachedAt)
{
    public DepartmentId Id { get; } = Id;
    public DateTime AttachedAt { get; } = AttachedAt;

    public static Result<DepartmentChildAttachment> Create(Guid id, DateTime attachedAt)
    {
        if (attachedAt == DateTime.MinValue || attachedAt == DateTime.MaxValue)
            return Error.ValidationError("Некорректное время закрепления подразделения.");

        Result<DepartmentId> departmentId = DepartmentId.Create(id);
        if (departmentId.IsFailure)
            return departmentId.Error;

        return new DepartmentChildAttachment(departmentId, attachedAt);
    }

    public static Result<DepartmentChildAttachment> FromJson(JsonElement json)
    {
        Guid id = json.GetProperty(nameof(Id)).GetGuid();
        DateTime attachedAt = json.GetProperty(nameof(AttachedAt)).GetDateTime();
        return Create(id, attachedAt);
    }
}