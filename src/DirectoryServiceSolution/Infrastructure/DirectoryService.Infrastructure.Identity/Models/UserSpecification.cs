namespace DirectoryService.Infrastructure.Identity.Models;

public sealed class UserSpecification
{
    public Guid? Id { get; }
    public string? Login { get; }
    public bool IsEmpty => Id == null && Login == null;

    private UserSpecification(Guid? id, string? login)
    {
        Id = id;
        Login = login;
    }

    public static UserSpecification Empty()
    {
        return new UserSpecification(null, null);
    }

    public UserSpecification WithId(Guid id)
    {
        return new UserSpecification(id, Login);
    }

    public UserSpecification WithLogin(string login)
    {
        return new UserSpecification(Id, login);
    }
}
