using System.Reflection.Metadata.Ecma335;

namespace DirectoryService.Core.DeparmentsContext.ValueObjects;

public readonly record struct DepartmentLifeCycle
{
    public DateTime? DeletedAt { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public DepartmentLifeCycle()
    {
        DeletedAt = null;
        DateTime now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    private DepartmentLifeCycle(DateTime? deletedAt, DateTime createdAt, DateTime updatedAt)
    {
        DeletedAt = deletedAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static DepartmentLifeCycle Create(DateTime? deletedAt, DateTime createdAt, DateTime updatedAt)
    {
        DateTime[] dates = [createdAt, updatedAt];
        if (dates.Any(d => d == default))
            throw new ApplicationException("Даты жизненного цикла подразделения некорректны.");
        return new DepartmentLifeCycle(deletedAt, createdAt, updatedAt);        
    }

    public DepartmentLifeCycle Update()
    {
        if (IsDeleted())
            throw new ApplicationException("Подразделение было удалено. Нельзя обновить.");
        return new DepartmentLifeCycle(DeletedAt, CreatedAt, DateTime.UtcNow);
    }

    public DepartmentLifeCycle Delete()
    {
        if (IsDeleted())
            throw new ApplicationException("Подразделение уже было удалено. Нельзя удалить.");
        return new DepartmentLifeCycle(DateTime.UtcNow, CreatedAt, UpdatedAt);
    }

    private bool IsDeleted()
    {
        return DeletedAt.HasValue;
    }
}
