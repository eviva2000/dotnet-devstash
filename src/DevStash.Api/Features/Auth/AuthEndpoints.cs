using System.ComponentModel.DataAnnotations;
using DevStash.Api.Data.Identity;
using DevStash.Api.Features.Auth.Contracts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace DevStash.Api.Features.Auth;

public static class AuthEndpoints
{
    private const int EmailMaxLength = 256;

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");

        group.MapGet("/csrf", GetCsrfToken)
            .AllowAnonymous()
            .Produces<CsrfTokenResponse>(StatusCodes.Status200OK)
            .WithName("GetAuthCsrfToken");

        group.MapPost("/register", Register)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .AllowAnonymous()
            .Produces<AuthenticatedUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("RegisterUser");

        group.MapPost("/login", Login)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .AllowAnonymous()
            .Produces<AuthenticatedUserResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithName("LoginUser");

        group.MapPost("/logout", Logout)
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("LogoutUser");

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .Produces<AuthenticatedUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("GetCurrentUser");

        return endpoints;
    }

    private static IResult GetCsrfToken(HttpContext context, IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Headers.CacheControl = "no-store";

        return TypedResults.Ok(new CsrfTokenResponse(
            tokens.RequestToken
            ?? throw new InvalidOperationException("Antiforgery did not issue a request token.")));
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var errors = ValidateRegistration(request, displayName, email);

        if (errors.Count > 0)
        {
            return ValidationProblem(errors);
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return DuplicateEmailProblem();
        }

        var user = new ApplicationUser
        {
            DisplayName = displayName,
            Email = email,
            UserName = email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(error =>
                    error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return DuplicateEmailProblem();
            }

            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["registration"] = ["Unable to create the account with the supplied values."]
                },
                extensions: ProblemCode("registration_failed"));
        }

        return Results.Json(
            AuthenticatedUserResponse.FromUser(user),
            statusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var errors = ValidateLogin(request, email);
        if (errors.Count > 0)
        {
            return ValidationProblem(errors);
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return InvalidCredentialsProblem();
        }

        var result = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return InvalidCredentialsProblem();
        }

        await signInManager.SignInAsync(user, request.RememberMe);
        return TypedResults.Ok(AuthenticatedUserResponse.FromUser(user));
    }

    private static async Task<IResult> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(context.User);
        return user is null
            ? TypedResults.Unauthorized()
            : TypedResults.Ok(AuthenticatedUserResponse.FromUser(user));
    }

    private static Dictionary<string, string[]> ValidateRegistration(
        RegisterRequest request,
        string displayName,
        string email)
    {
        var errors = new Dictionary<string, string[]>();

        if (displayName.Length == 0)
        {
            errors["displayName"] = ["Display name is required."];
        }
        else if (displayName.Length > ApplicationUser.DisplayNameMaxLength)
        {
            errors["displayName"] =
                [$"Display name must be {ApplicationUser.DisplayNameMaxLength} characters or fewer."];
        }

        AddEmailErrors(errors, email);

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["Password is required."];
        }
        else if (request.Password.Length < 8)
        {
            errors["password"] = ["Password must be at least 8 characters."];
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            errors["confirmPassword"] = ["Password confirmation must match the password."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateLogin(LoginRequest request, string email)
    {
        var errors = new Dictionary<string, string[]>();
        AddEmailErrors(errors, email);

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        return errors;
    }

    private static void AddEmailErrors(Dictionary<string, string[]> errors, string email)
    {
        if (email.Length == 0)
        {
            errors["email"] = ["Email is required."];
        }
        else if (email.Length > EmailMaxLength || !new EmailAddressAttribute().IsValid(email))
        {
            errors["email"] = ["Email must be a valid email address."];
        }
    }

    private static IResult ValidationProblem(Dictionary<string, string[]> errors) =>
        Results.ValidationProblem(errors, extensions: ProblemCode("validation_failed"));

    private static IResult DuplicateEmailProblem() =>
        Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Email already registered",
            detail: "An account is already registered with this email address.",
            extensions: ProblemCode("email_already_registered"));

    private static IResult InvalidCredentialsProblem() =>
        Results.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Invalid credentials",
            detail: "The email or password is invalid.",
            extensions: ProblemCode("invalid_credentials"));

    private static Dictionary<string, object?> ProblemCode(string code) =>
        new() { ["code"] = code };
}
