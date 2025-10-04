using ResultLibrary;

namespace DirectoryService.Core.Common.ValueObjects;

public readonly record struct EntityLifeCycle
{
    public DateTime? DeletedAt { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public bool IsDeleted => DeletedAt != null;

    public EntityLifeCycle()
    {
        DeletedAt = null;
        DateTime now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    private EntityLifeCycle(DateTime? deletedAt, DateTime createdAt, DateTime updatedAt)
    {
        DeletedAt = deletedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<EntityLifeCycle> Create(
        DateTime? deletedAt,
        DateTime createdAt,
        DateTime updatedAt
    )
    {
        DateTime[] dates = [createdAt, updatedAt];
        return dates.Any(d => d == default)
            ? Error.ValidationError("Даты жизненного цикла некорректны.")
            : new EntityLifeCycle(deletedAt, createdAt, updatedAt);
    }

    public Result<EntityLifeCycle> Update() =>
        IsDeleted
            ? Error.ConflictError("Запись была удалена. Нельзя обновить.")
            : new EntityLifeCycle(DeletedAt, CreatedAt, DateTime.UtcNow);

    public Result<EntityLifeCycle> Delete() =>
        IsDeleted
            ? Error.ConflictError("Запись уже было удалена. Нельзя удалить.")
            : new EntityLifeCycle(DateTime.UtcNow, CreatedAt, UpdatedAt);
}
