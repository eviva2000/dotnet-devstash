using DevStash.Api.Data.Identity;

namespace DevStash.Api.Features.Auth.Contracts;

public sealed record AuthenticatedUserResponse(Guid Id, string DisplayName, string Email)
{
    public static AuthenticatedUserResponse FromUser(ApplicationUser user) =>
        new(user.Id, user.DisplayName, user.Email ?? string.Empty);
}
