using System.Text.Json;
using DirectoryService.Core.DeparmentsContext.ValueObjects;

namespace DirectoryService.Infrastructure.PostgreSQL.Snapshots;

public sealed record DepartmentChildAttachmentSnapshot(Guid Id, DateTime AttachedAt)
{
    public static DepartmentChildAttachmentSnapshot FromAttachment(
        DepartmentChildAttachment attachment
    )
    {
        return new DepartmentChildAttachmentSnapshot(attachment.Id.Value, attachment.AttachedAt);
    }

    public DepartmentChildAttachment ToAttachment()
    {
        return DepartmentChildAttachment.Create(Id, AttachedAt);
    }
}

public sealed record DepartmentAttachmentsSnapshot(
    IReadOnlyList<DepartmentChildAttachmentSnapshot> Attachments
)
{
    public static DepartmentAttachmentsSnapshot FromHistory(
        DepartmentChildAttachmentsHistory history
    )
    {
        return new DepartmentAttachmentsSnapshot(
            [.. history.Attachments.Select(DepartmentChildAttachmentSnapshot.FromAttachment)]
        );
    }

    public static DepartmentAttachmentsSnapshot FromJson(string json)
    {
        DepartmentAttachmentsSnapshot? snapshot =
            JsonSerializer.Deserialize<DepartmentAttachmentsSnapshot>(
                json,
                JsonSerializerOptions.Default
            );

        if (snapshot == null)
        {
            throw new ApplicationException(
                $"Некорректный JSON для {nameof(DepartmentChildAttachmentsHistory)}."
            );
        }

        return snapshot;
    }

    public DepartmentChildAttachmentsHistory ToHistory()
    {
        return new DepartmentChildAttachmentsHistory(
            [.. Attachments.Select(a => a.ToAttachment())]
        );
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonSerializerOptions.Default);
    }

    public bool IsSameAs(DepartmentAttachmentsSnapshot other)
    {
        if (Attachments.Count != other.Attachments.Count)
        {
            return false;
        }

        for (int index = 0; index < Attachments.Count; index++)
        {
            if (Attachments[index] != other.Attachments[index])
            {
                return false;
            }
        }

        return true;
    }
}
