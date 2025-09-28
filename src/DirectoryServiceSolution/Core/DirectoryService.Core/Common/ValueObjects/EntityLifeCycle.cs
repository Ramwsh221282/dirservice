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

    public static EntityLifeCycle Create(DateTime? deletedAt, DateTime createdAt, DateTime updatedAt)
    {
        DateTime[] dates = [createdAt, updatedAt];
        if (dates.Any(d => d == default))
            throw new ApplicationException("Даты жизненного цикла некорректны.");
        return new EntityLifeCycle(deletedAt, createdAt, updatedAt);        
    }

    public EntityLifeCycle Update()
    {
        if (IsDeleted)
            throw new ApplicationException("Запись была удалена. Нельзя обновить.");
        return new EntityLifeCycle(DeletedAt, CreatedAt, DateTime.UtcNow);
    }

    public EntityLifeCycle Delete()
    {
        if (IsDeleted)
            throw new ApplicationException("Запись уже было удалена. Нельзя удалить.");
        return new EntityLifeCycle(DateTime.UtcNow, CreatedAt, UpdatedAt);
    }
}
