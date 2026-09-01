using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Jwt;

public sealed record ValidatedToken(Guid UserId, string Login, DateTime ExpiresAt);

public interface ITokenValidator
{
    Result<ValidatedToken> Validate(string accessToken);
}
