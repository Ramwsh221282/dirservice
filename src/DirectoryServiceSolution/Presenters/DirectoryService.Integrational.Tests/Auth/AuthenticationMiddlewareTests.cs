using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DirectoryService.Infrastructure.Identity.Jwt;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Integrational.Tests.Auth;

public sealed class AuthenticationMiddlewareTests : IClassFixture<TestApplicationFactory>, IAsyncLifetime
{
    private const string ProtectedEndpoint = "/api/locations";

    private readonly TestApplicationFactory _factory;

    public AuthenticationMiddlewareTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabase();
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Protected_Endpoint_Without_Token_Returns_401()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(ProtectedEndpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Protected_Endpoint_With_Valid_Token_Is_Allowed()
    {
        HttpClient client = _factory.CreateClient();
        string accessToken = await SignUpAndSignIn(client, "valid-user", "password123");

        HttpRequestMessage request = new(HttpMethod.Get, ProtectedEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Protected_Endpoint_With_Expired_Token_Returns_401()
    {
        HttpClient client = _factory.CreateClient();

        JwtOptions options = _factory.Services.GetRequiredService<JwtOptions>();
        JwtOptions expiredOptions = new()
        {
            SecretKey = options.SecretKey,
            AccessTokenLifetimeMinutes = -1,
            RefreshTokenLifetimeDays = 7,
        };

        JwtProvider provider = new(expiredOptions);
        GeneratedTokens tokens = provider.GenerateTokens(Guid.NewGuid(), "expired-user");

        HttpRequestMessage request = new(HttpMethod.Get, ProtectedEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Protected_Endpoint_With_Foreign_Key_Token_Returns_401()
    {
        HttpClient client = _factory.CreateClient();

        JwtOptions foreignOptions = new()
        {
            SecretKey = "completely-different-secret-key-0123456789",
            AccessTokenLifetimeMinutes = 5,
            RefreshTokenLifetimeDays = 7,
        };

        JwtProvider provider = new(foreignOptions);
        GeneratedTokens tokens = provider.GenerateTokens(Guid.NewGuid(), "intruder");

        HttpRequestMessage request = new(HttpMethod.Get, ProtectedEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_Token_Is_Not_Accepted_By_Protected_Endpoint()
    {
        HttpClient client = _factory.CreateClient();

        await SignUp(client, "refresh-user", "password123");
        JsonElement signIn = await SignIn(client, "refresh-user", "password123");
        string refreshToken = signIn.GetProperty("token").GetProperty("refreshToken").GetString()!;

        HttpRequestMessage request = new(HttpMethod.Get, ProtectedEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Auth_Endpoints_Stay_Anonymous()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/sign-up",
            new { Login = "anonymous-check", Password = "password123" }
        );

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("token-without-scheme")]
    [InlineData("Basic dXNlcjpwYXNz")]
    [InlineData("Bearer")]
    [InlineData("Bearer ")]
    [InlineData("Bearer    ")]
    public async Task Malformed_Authorization_Header_Returns_401(string headerValue)
    {
        HttpClient client = _factory.CreateClient();

        HttpRequestMessage request = new(HttpMethod.Get, ProtectedEndpoint);
        request.Headers.TryAddWithoutValidation("Authorization", headerValue);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Bearer_Scheme_Is_Case_Insensitive()
    {
        HttpClient client = _factory.CreateClient();
        string accessToken = await SignUpAndSignIn(client, "case-user", "password123");

        HttpRequestMessage request = new(HttpMethod.Get, ProtectedEndpoint);
        request.Headers.TryAddWithoutValidation("Authorization", "bearer " + accessToken);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Rotated_Access_Token_Is_Accepted_After_Refresh()
    {
        HttpClient client = _factory.CreateClient();

        await SignUp(client, "rotate-user", "password123");
        JsonElement signIn = await SignIn(client, "rotate-user", "password123");
        string refreshToken = signIn.GetProperty("token").GetProperty("refreshToken").GetString()!;

        HttpResponseMessage refreshResponse = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { RefreshToken = refreshToken }
        );

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        JsonElement refreshed = await ReadValue(refreshResponse);
        string newAccessToken = refreshed.GetProperty("accessToken").GetString()!;

        HttpRequestMessage request = new(HttpMethod.Get, ProtectedEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<string> SignUpAndSignIn(HttpClient client, string login, string password)
    {
        await SignUp(client, login, password);
        JsonElement signIn = await SignIn(client, login, password);
        return signIn.GetProperty("token").GetProperty("accessToken").GetString()!;
    }

    private static async Task SignUp(HttpClient client, string login, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/sign-up",
            new { Login = login, Password = password }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<JsonElement> SignIn(HttpClient client, string login, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { Login = login, Password = password }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadValue(response);
    }

    private static async Task<JsonElement> ReadValue(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("value").Clone();
    }
}
