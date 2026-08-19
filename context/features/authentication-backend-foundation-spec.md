# Authentication Backend Foundation

## Goal

Add secure email-and-password authentication to the DevStash API using the ASP.NET Core Identity persistence created during Database Foundation.

This feature delivers the backend authentication contract only: registration, cookie sign-in, sign-out, current-user lookup, authorization middleware, CSRF protection, and focused tests. React authentication screens belong to the next feature.

## Learning Outcomes

By completing this feature, we should understand:

- The difference between authentication and authorization.
- How `UserManager<TUser>` creates users and hashes passwords.
- How `SignInManager<TUser>` validates credentials and creates authentication cookies.
- How authentication middleware turns a cookie into `HttpContext.User` and claims.
- How `RequireAuthorization()` protects a Minimal API endpoint.
- Why browser-based first-party applications should prefer an HttpOnly cookie over storing a token in JavaScript.
- Why cookie-authenticated write requests need CSRF protection.
- Why an API should return generic login failures rather than revealing whether an email exists.

## Existing Foundation

Reuse the existing:

- `ApplicationUser : IdentityUser<Guid>` entity.
- `DevStashDbContext` Identity persistence.
- `devstash_dotnet` PostgreSQL schema.
- Npgsql and Neon development connection configuration.
- Application liveness endpoint at `GET /health`.

Do not create a second user entity, authentication database, or migrations-history table.

## Architecture Decisions

- Use ASP.NET Core Identity for user creation, password hashing, credential validation, lockout state, and cookie sign-in.
- Use encrypted HttpOnly authentication cookies, not JWTs or tokens stored in `localStorage`.
- Add custom Minimal API endpoints under `/api/auth` instead of mapping the full built-in Identity API endpoint set. This keeps unimplemented email confirmation, password reset, and 2FA endpoints unavailable.
- Keep the API and future React application same-origin in production. The later frontend feature will use the Vite development proxy and include credentials automatically.
- Use DTOs for every request and response. Never serialize `ApplicationUser`, password hashes, security stamps, or Identity internals directly.

## User Model Change

Extend `ApplicationUser` with:

- Required `DisplayName` with a documented maximum length.

Continue using the normalized email as the login identifier and set `UserName` to the normalized application email through Identity APIs.

Update the EF configuration so normalized email is unique. Generate a new migration that changes only objects inside `devstash_dotnet`.

## Identity Configuration

Configure Identity explicitly:

- Require a unique email.
- Require a password of at least 8 characters.
- Keep the remaining password rules explicit and documented rather than relying on hidden defaults.
- Enable lockout for password failures.
- Lock an account after 5 failed attempts for 15 minutes.
- Do not require confirmed email in this feature.
- Add `SignInManager<ApplicationUser>` and Identity cookie handlers to the existing Identity registration.
- Add authorization services.

Add middleware in the correct order:

1. HTTPS redirection
2. Authentication
3. Authorization
4. Endpoint execution

## Cookie Requirements

Use a dedicated application cookie with:

- Name: `devstash.auth`
- `HttpOnly = true`
- `Secure = true`
- `SameSite = Lax`
- A documented finite lifetime
- Sliding expiration
- Persistent lifetime only when `RememberMe` is requested

API authentication failures must return `401 Unauthorized` or `403 Forbidden` without redirecting to an HTML login page.

Do not return the cookie value in JSON and do not log cookies, passwords, password hashes, CSRF tokens, or security stamps.

## CSRF Contract

Cookie authentication automatically sends credentials with requests, so state-changing endpoints require request-forgery protection.

- Register ASP.NET Core antiforgery services.
- Add `GET /api/auth/csrf` as an anonymous endpoint that issues an antiforgery cookie and returns the corresponding request token in a small DTO.
- Require the request token in a documented header such as `X-XSRF-TOKEN` for registration, login, and logout.
- Reject missing or invalid tokens with a consistent client-safe response.
- Do not rely on CORS alone as CSRF protection.

The future React auth feature will fetch this token before submitting authentication forms.

## Endpoint Contract

Group endpoints under `/api/auth` and give each endpoint a stable route name.

### `GET /api/auth/csrf`

Anonymous.

Response `200 OK`:

```json
{
  "requestToken": "..."
}
```

The response also stores the paired antiforgery cookie.

### `POST /api/auth/register`

Anonymous, with a valid CSRF token.

Request:

```json
{
  "displayName": "Ada Lovelace",
  "email": "ada@example.com",
  "password": "example-password",
  "confirmPassword": "example-password"
}
```

Behavior:

- Trim display name and email.
- Validate display name, email format, password policy, and matching confirmation.
- Create the user through `UserManager<ApplicationUser>` so Identity hashes the password.
- Do not sign the user in automatically.

Responses:

- `201 Created` with a safe user response.
- `400 Bad Request` with validation problem details.
- `409 Conflict` when the normalized email is already registered.

### `POST /api/auth/login`

Anonymous, with a valid CSRF token.

Request:

```json
{
  "email": "ada@example.com",
  "password": "example-password",
  "rememberMe": false
}
```

Behavior:

- Validate credentials through Identity.
- Count failed attempts toward lockout.
- Return the same generic failure for unknown email, wrong password, and locked account.
- On success, issue the authentication cookie and return the safe current-user response.

Responses:

- `200 OK` with the authenticated user response.
- `400 Bad Request` for malformed input.
- `401 Unauthorized` with a generic invalid-credentials problem.

### `POST /api/auth/logout`

Requires authentication and a valid CSRF token.

Behavior:

- Sign out through Identity.
- Expire the authentication cookie.

Response:

- `204 No Content`.

### `GET /api/auth/me`

Requires authentication.

Response `200 OK`:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "displayName": "Ada Lovelace",
  "email": "ada@example.com"
}
```

An anonymous request returns `401 Unauthorized` with no redirect location.

## Error Contract

- Use ASP.NET Core Problem Details or Validation Problem Details consistently.
- Return stable machine-readable error codes where the frontend needs to distinguish outcomes.
- Do not expose Identity's internal exception messages or database errors.
- Do not reveal whether an account exists during login.
- Registration may report that an email is already registered because the user is explicitly attempting to claim it.

## Suggested Structure

```text
src/DevStash.Api/
├── Features/
│   └── Auth/
│       ├── AuthEndpoints.cs
│       └── Contracts/
│           ├── AuthenticatedUserResponse.cs
│           ├── CsrfTokenResponse.cs
│           ├── LoginRequest.cs
│           └── RegisterRequest.cs
├── Data/
│   ├── Configurations/ApplicationUserConfiguration.cs
│   ├── Identity/ApplicationUser.cs
│   └── Migrations/
└── Program.cs
```

Keep endpoint registration outside `Program.cs` through a focused extension method. Do not add controllers or additional architecture projects for this feature.

## Migration Safety

- Generate a descriptive migration such as `AddAuthenticationUserFields`.
- Review the generated migration and SQL before applying it.
- Confirm that every affected object is inside `devstash_dotnet`.
- Apply only to Neon development branch `br-aged-queen-abnc2h82`.
- Do not inspect or touch production branch `br-rapid-rain-ab4b2m8z`.
- Do not alter the existing Prisma-managed `public` schema.

## Testing and Verification

Use an isolated test database/provider for endpoint tests. Automated tests must not create users in production or depend on the live Neon development database unless a test is explicitly marked and invoked as an integration smoke test.

Add focused integration tests covering:

- CSRF token issuance.
- Registration success and password hashing.
- Registration validation and duplicate normalized email.
- Successful login and authentication-cookie issuance.
- Generic failure for unknown email and wrong password.
- Account lockout after the configured failed-attempt limit.
- Anonymous `GET /api/auth/me` returns `401` without a redirect.
- Authenticated `GET /api/auth/me` returns only the safe user contract.
- Logout expires the session and a later `/me` request returns `401`.
- Missing or invalid CSRF tokens are rejected on state-changing endpoints.

Run and report:

- `dotnet restore` when packages change.
- `dotnet build --no-restore`.
- `dotnet test --no-build` after a successful build.
- Migration generation and SQL review.
- `dotnet ef migrations has-pending-model-changes` after generating the migration.

## Acceptance Criteria

- Registration creates an Identity user with a hashed password and no automatic session.
- Login issues a secure HttpOnly cookie without returning credentials in JSON.
- Logout invalidates the session.
- `/api/auth/me` is protected and exposes only ID, display name, and email.
- Authentication and authorization middleware are registered in the correct order.
- Protected API endpoints return `401/403` rather than HTML redirects.
- State-changing authentication endpoints enforce CSRF protection.
- Duplicate registration and invalid login behavior follow the documented error contract.
- Failed-login lockout is configured and tested.
- The user-model migration affects only `devstash_dotnet` on the Neon development branch.
- Existing health, database-model, and database-registration tests continue to pass.
- New authentication tests pass without touching production.

## Out of Scope

- React sign-in and registration pages
- Client-side auth state and protected React routes
- GitHub or other external OAuth providers
- Email verification
- Forgot-password and reset-password flows
- Two-factor authentication
- Role-based product permissions
- Auth endpoint rate limiting
- Profile management and password changes
- Item ownership endpoints
- Production deployment or migration

The API must not be treated as production-ready until rate limiting, email verification, recovery flows, and deployment-specific data-protection key storage are addressed.

## References

- ASP.NET Core Identity: https://learn.microsoft.com/aspnet/core/security/authentication/identity
- ASP.NET Core authentication overview: https://learn.microsoft.com/aspnet/core/security/authentication/
- Cookie authentication for APIs: https://learn.microsoft.com/aspnet/core/security/authentication/api-endpoint-auth
