namespace DirectoryService.Infrastructure.Identity.Hashing;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}
