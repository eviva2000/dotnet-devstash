namespace DevStash.Api.Features.Auth.Contracts;

public sealed record RegisterRequest(
    string DisplayName,
    string Email,
    string Password,
    string ConfirmPassword);
