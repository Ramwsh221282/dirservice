namespace DirectoryService.Infrastructure.Identity.Models;

public sealed class UserSnapshot
{
    public Guid Id { get; }
    public string Login { get; }
    public string Password { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public DateTime? DeletedAt { get; }

    private UserSnapshot(
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

    public static UserSnapshot FromUser(User user)
    {
        return new UserSnapshot(
            user.Id,
            user.Login,
            user.Password,
            user.CreatedAt,
            user.UpdatedAt,
            user.DeletedAt
        );
    }
}
