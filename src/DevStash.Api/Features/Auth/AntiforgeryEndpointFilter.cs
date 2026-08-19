using Microsoft.AspNetCore.Antiforgery;

namespace DevStash.Api.Features.Auth;

public sealed class AntiforgeryEndpointFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid CSRF token",
                detail: "A valid request-forgery token is required.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "invalid_csrf_token"
                });
        }

        return await next(context);
    }
}
