namespace DirectoryService.Contracts.Auth;

public sealed record SignInRequest(string Login, string Password);
