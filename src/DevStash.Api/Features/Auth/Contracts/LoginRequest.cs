namespace DevStash.Api.Features.Auth.Contracts;

public sealed record LoginRequest(string Email, string Password, bool RememberMe);
