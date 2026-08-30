namespace DirectoryService.Infrastructure.Identity.Models;

public sealed class User
{
    public Guid Id { get; }
    public string Login { get; }
    public string Password { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public DateTime? DeletedAt { get; }

    public User(
        Guid id,
        string login,
        string password,
        DateTime createdAt,
        DateTime updatedAt,
        DateTime? deletedAt)
    {
        Id = id;
        Login = login;
        Password = password;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static User CreateNew(string login, string hashedPassword)
    {
        DateTime now = DateTime.UtcNow;
        return new User(Guid.NewGuid(), login, hashedPassword, now, now, null);
    }
}
