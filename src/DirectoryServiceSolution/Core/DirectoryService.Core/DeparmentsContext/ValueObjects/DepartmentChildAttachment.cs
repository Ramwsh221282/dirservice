using System.Text.Json;
using ResultLibrary;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

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

    public static DepartmentChildAttachment FromJson(JsonElement json)
    {
        try
        {
            Guid id = json.GetProperty(nameof(Id)).GetProperty("Value").GetGuid();
            DateTime attachedAt = json.GetProperty(nameof(AttachedAt)).GetDateTime();
            return Create(id, attachedAt);
        }
        catch (JsonException)
        {
            string message = $"Некорректный маппинг из JSON в {nameof(DepartmentChildAttachment)}.";
            throw new ApplicationException(message);
        }
    }
}
