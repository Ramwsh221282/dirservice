using System.Text.Json;
using ResultLibrary;

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

    public int Count() => _attachments.Count;

    public bool IsAttached(DepartmentId departmentId) =>
        _attachments.Any(a => a.Id == departmentId);

    public bool IsAttached(Department department) => IsAttached(department.Id);

    public static DepartmentChildAttachmentsHistory Empty() => new();

    public static DepartmentChildAttachmentsHistory FromJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement attachmentsJson = document.RootElement.GetProperty(nameof(Attachments));

        List<DepartmentChildAttachment> attachments = [];

        foreach (JsonElement entry in attachmentsJson.EnumerateArray())
        {
            Result<DepartmentChildAttachment> attachment = DepartmentChildAttachment.FromJson(
                entry
            );
            if (attachment.IsFailure)
                throw new ApplicationException(
                    $"Некорректный JSON для {nameof(DepartmentChildAttachment)}"
                );
            attachments.Add(attachment);
        }

        return new DepartmentChildAttachmentsHistory(attachments);
    }
}
