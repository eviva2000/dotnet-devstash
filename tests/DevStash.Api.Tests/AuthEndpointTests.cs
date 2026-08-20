using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DevStash.Api.Data.Identity;
using DevStash.Api.Features.Auth.Contracts;
using DevStash.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DevStash.Api.Tests;

public sealed class AuthEndpointTests : IClassFixture<AuthWebApplicationFactory>
{
    private const string Password = "example-password";
    private readonly AuthWebApplicationFactory _factory;

    public AuthEndpointTests(AuthWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Csrf_WhenRequested_IssuesCookieAndRequestToken()
    {
        using var client = _factory.CreateHttpsClient();

        var response = await client.GetAsync("/api/auth/csrf");
        var body = await response.Content.ReadFromJsonAsync<CsrfTokenResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestToken));
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            value => value.Contains("devstash.csrf=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Register_WithValidInput_CreatesHashedUserWithoutSession()
    {
        using var client = _factory.CreateHttpsClient();
        var email = UniqueEmail("registration");
        var token = await GetCsrfToken(client);

        var response = await Register(client, token, "  Ada Lovelace  ", $"  {email}  ");

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var storedUser = await userManager.FindByEmailAsync(email);

        Assert.NotNull(storedUser);
        Assert.NotNull(storedUser.PasswordHash);
        Assert.NotEqual(Password, storedUser.PasswordHash);
        Assert.True(await userManager.CheckPasswordAsync(storedUser, Password));

        var meResponse = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidInput_ReturnsValidationProblem()
    {
        using var client = _factory.CreateHttpsClient();
        var token = await GetCsrfToken(client);

        var response = await PostAsJsonWithCsrf(
            client,
            "/api/auth/register",
            new RegisterRequest(" ", "not-an-email", "short", "different"),
            token);
        using var body = await ReadJson(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", body.RootElement.GetProperty("code").GetString());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("displayName", out _));
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("email", out _));
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("password", out _));
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("confirmPassword", out _));
    }

    [Fact]
    public async Task Register_WithSameNormalizedEmail_ReturnsIndistinguishableAcceptedResponse()
    {
        using var client = _factory.CreateHttpsClient();
        var email = UniqueEmail("duplicate");
        var token = await GetCsrfToken(client);
        var first = await Register(client, token, "First User", email);

        var second = await Register(client, token, "Second User", email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(first.StatusCode, second.StatusCode);
        Assert.Empty(await first.Content.ReadAsStringAsync());
        Assert.Empty(await second.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_WithValidCredentials_IssuesSecureCookieAndReturnsSafeUser()
    {
        var email = UniqueEmail("login");
        await CreateUser(email, "Grace Hopper");
        using var client = _factory.CreateHttpsClient();
        var token = await GetCsrfToken(client);

        var response = await Login(client, token, email, Password);
        var body = await response.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();
        var authCookie = response.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("devstash.auth=", StringComparison.Ordinal));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Grace Hopper", body.DisplayName);
        Assert.Equal(email, body.Email);
        Assert.Contains("httponly", authCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", authCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", authCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expires=", authCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithRememberMe_IssuesPersistentCookieWithFiniteLifetime()
    {
        var email = UniqueEmail("remember-me");
        await CreateUser(email, "Remembered User");
        using var client = _factory.CreateHttpsClient();
        var token = await GetCsrfToken(client);

        var response = await Login(client, token, email, Password, rememberMe: true);
        var authCookie = response.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("devstash.auth=", StringComparison.Ordinal));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("expires=", authCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_ForUnknownEmailAndWrongPassword_ReturnsSameGenericFailure()
    {
        var email = UniqueEmail("wrong-password");
        await CreateUser(email, "Known User");
        using var client = _factory.CreateHttpsClient();
        var token = await GetCsrfToken(client);

        var unknownResponse = await Login(client, token, UniqueEmail("unknown"), "wrong-password");
        var wrongResponse = await Login(client, token, email, "wrong-password");
        using var unknownBody = await ReadJson(unknownResponse);
        using var wrongBody = await ReadJson(wrongResponse);

        Assert.Equal(HttpStatusCode.Unauthorized, unknownResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongResponse.StatusCode);
        Assert.Equal(
            unknownBody.RootElement.GetProperty("code").GetString(),
            wrongBody.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            unknownBody.RootElement.GetProperty("detail").GetString(),
            wrongBody.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Login_AfterFiveFailures_LocksAccountAndStillReturnsGenericFailure()
    {
        var email = UniqueEmail("lockout");
        await CreateUser(email, "Lockout User");
        using var client = _factory.CreateHttpsClient();
        var token = await GetCsrfToken(client);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var failedResponse = await Login(client, token, email, "wrong-password");
            Assert.Equal(HttpStatusCode.Unauthorized, failedResponse.StatusCode);
        }

        var lockedResponse = await Login(client, token, email, Password);
        using var lockedBody = await ReadJson(lockedResponse);

        Assert.Equal(HttpStatusCode.Unauthorized, lockedResponse.StatusCode);
        Assert.Equal("invalid_credentials", lockedBody.RootElement.GetProperty("code").GetString());

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var storedUser = await userManager.FindByEmailAsync(email);
        Assert.NotNull(storedUser);
        Assert.True(storedUser.LockoutEnd > DateTimeOffset.UtcNow.AddMinutes(14));
    }

    [Fact]
    public async Task Me_WithoutSession_ReturnsUnauthorizedWithoutRedirect()
    {
        using var client = _factory.CreateHttpsClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Me_WithSession_ReturnsOnlySafeUserContract()
    {
        var email = UniqueEmail("me");
        await CreateUser(email, "Current User");
        using var client = _factory.CreateHttpsClient();
        var token = await GetCsrfToken(client);
        var loginResponse = await Login(client, token, email, Password);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var response = await client.GetAsync("/api/auth/me");
        using var body = await ReadJson(response);
        var properties = body.RootElement.EnumerateObject().Select(property => property.Name).Order().ToArray();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["displayName", "email", "id"], properties);
        Assert.Equal("Current User", body.RootElement.GetProperty("displayName").GetString());
        Assert.Equal(email, body.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Logout_WithValidSession_ExpiresAuthentication()
    {
        var email = UniqueEmail("logout");
        await CreateUser(email, "Logout User");
        using var client = _factory.CreateHttpsClient();
        var loginToken = await GetCsrfToken(client);
        var loginResponse = await Login(client, loginToken, email, Password);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var logoutToken = await GetCsrfToken(client);

        var logoutResponse = await PostWithCsrf(client, "/api/auth/logout", logoutToken);
        var meResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    [Fact]
    public async Task StateChangingEndpoints_WithMissingOrInvalidCsrfToken_ReturnBadRequest()
    {
        using var client = _factory.CreateHttpsClient();
        var missingResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Missing Token", UniqueEmail("missing-csrf"), Password, Password));

        await GetCsrfToken(client);
        var invalidResponse = await PostAsJsonWithCsrf(
            client,
            "/api/auth/login",
            new LoginRequest(UniqueEmail("invalid-csrf"), Password, false),
            "invalid-token");
        using var missingBody = await ReadJson(missingResponse);
        using var invalidBody = await ReadJson(invalidResponse);

        Assert.Equal(HttpStatusCode.BadRequest, missingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        Assert.Equal("invalid_csrf_token", missingBody.RootElement.GetProperty("code").GetString());
        Assert.Equal("invalid_csrf_token", invalidBody.RootElement.GetProperty("code").GetString());
    }

    private async Task CreateUser(string email, string displayName)
    {
        _factory.CreateHttpsClient().Dispose();
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var result = await userManager.CreateAsync(
            new ApplicationUser
            {
                DisplayName = displayName,
                Email = email,
                UserName = email
            },
            Password);

        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Description)));
    }

    private static async Task<string> GetCsrfToken(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<CsrfTokenResponse>();
        return Assert.IsType<string>(body?.RequestToken);
    }

    private static Task<HttpResponseMessage> Register(
        HttpClient client,
        string token,
        string displayName,
        string email) =>
        PostAsJsonWithCsrf(
            client,
            "/api/auth/register",
            new RegisterRequest(displayName, email, Password, Password),
            token);

    private static Task<HttpResponseMessage> Login(
        HttpClient client,
        string token,
        string email,
        string password,
        bool rememberMe = false) =>
        PostAsJsonWithCsrf(
            client,
            "/api/auth/login",
            new LoginRequest(email, password, rememberMe),
            token);

    private static async Task<HttpResponseMessage> PostAsJsonWithCsrf<T>(
        HttpClient client,
        string requestUri,
        T value,
        string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add("X-XSRF-TOKEN", token);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostWithCsrf(
        HttpClient client,
        string requestUri,
        string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Add("X-XSRF-TOKEN", token);
        return await client.SendAsync(request);
    }

    private static async Task<JsonDocument> ReadJson(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync());

    private static string UniqueEmail(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}@example.com";
}
