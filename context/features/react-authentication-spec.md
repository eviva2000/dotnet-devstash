# React Authentication

## Goal

Connect the React application to the existing ASP.NET Core authentication API and deliver a complete browser authentication flow: registration, login, session restoration, logout, and a protected application shell.

This feature replaces the frontend foundation screen with the first usable product flow. It consumes the existing backend contract without changing the database model or adding new authentication methods.

## Learning Outcomes

By completing this feature, we should understand:

- How a Vite development proxy preserves a same-origin-style `/api` contract during local development.
- How an HttpOnly authentication cookie differs from authentication state held in React memory.
- How the application restores its user state with `GET /api/auth/me` after a refresh.
- How to acquire and send an antiforgery request token for cookie-authenticated writes.
- How controlled forms, client validation, API validation, and submission state work together.
- How protected navigation waits for authentication to resolve before redirecting.
- How to test user-visible React behavior without coupling tests to component internals.

## Existing Foundation

Reuse the existing:

- React 19, TypeScript, and Vite application in `src/devstash-web`.
- `GET /api/auth/csrf` antiforgery-token endpoint.
- `POST /api/auth/register`, `POST /api/auth/login`, and `POST /api/auth/logout` endpoints.
- Protected `GET /api/auth/me` endpoint.
- `AuthenticatedUserResponse` shape containing `id`, `displayName`, and `email`.
- Stable problem codes including `validation_failed`, `email_already_registered`, and `invalid_credentials`.
- Secure `devstash.auth` cookie configured by the API.

Do not recreate authentication in the frontend, read the HttpOnly cookie, or persist passwords, cookies, antiforgery tokens, or the authenticated-user response in `localStorage` or `sessionStorage`.

## Scope

Deliver:

- `/login` and `/register` routes.
- A protected `/app` route with a minimal authenticated shell.
- Session restoration when the React application starts.
- Registration, login, and logout against the real API contract.
- Redirects that prevent authenticated users from remaining on auth pages and anonymous users from viewing `/app`.
- Accessible validation, loading, error, and submission states.
- Focused frontend tests for the critical flows.

The protected shell is a destination and proof of authentication only. Item, collection, tag, search, and dashboard data belong to later features.

## Frontend Architecture Decisions

- Add a small, explicit client-side router suitable for React 19. Use declarative routes rather than manual `window.location` branching.
- Keep authenticated-user state in a focused auth provider/hook. The provider owns the initial `/me` request and exposes `user`, an initialization status, `login`, `register`, and `logout` operations.
- Centralize API calls, JSON parsing, and problem-details normalization in a typed auth client rather than duplicating `fetch` logic in components.
- Use relative `/api/...` URLs. Configure Vite to proxy `/api` to the local HTTPS API target; do not hard-code a production hostname.
- Send JSON with the appropriate content type. Same-origin requests use browser cookies normally; if fetch configuration makes credential behavior explicit, use `credentials: "same-origin"` rather than enabling broad cross-origin assumptions.
- Keep dependencies proportionate. Do not add a general server-state library, global state library, CSS framework, or component suite solely for these forms.

## Route and Navigation Behavior

### `/`

Use a deterministic entry route:

- While session restoration is pending, show the application loading state.
- An authenticated user continues to `/app`.
- An anonymous user continues to `/login`.

### `/login`

- Show email, password, and remember-me fields.
- Link to `/register`.
- On successful login, navigate to `/app` with replace semantics.
- If the user is already authenticated, navigate to `/app`.

### `/register`

- Show display name, email, password, and password-confirmation fields.
- Link to `/login`.
- Registration does not create a session. On success, navigate to `/login` and show a clear one-time success message.
- Do not automatically log in with the submitted password.
- If the user is already authenticated, navigate to `/app`.

### `/app`

- Require a restored authenticated user.
- Show a minimal DevStash shell with the user's display name and email plus a logout action.
- Do not briefly render protected content while `/me` is unresolved.
- An anonymous user navigates to `/login` with replace semantics.

Unknown routes should resolve predictably to the appropriate authenticated or anonymous entry point; a full not-found experience is outside this feature.

## Session Restoration

On application startup, request `GET /api/auth/me` once through the auth provider.

- `200 OK`: store the safe user DTO in React memory and mark initialization complete.
- `401 Unauthorized`: treat the visitor as anonymous; this is an expected state, not a visible global error.
- Network failure or unexpected response: show a recoverable application error with a retry action rather than treating it as a logged-out session.

React Strict Mode may run development effects more than once. The implementation must avoid harmful duplicate state transitions and should cancel or ignore stale requests during unmount. Do not weaken Strict Mode to hide lifecycle mistakes.

## CSRF Request Flow

Every registration, login, and logout attempt must:

1. Request `GET /api/auth/csrf` immediately before the write.
2. Read `requestToken` from the response.
3. Send it in the API's configured `X-XSRF-TOKEN` request header.
4. Send the write request using the paired antiforgery cookie.

Keep the request token in memory only for the operation. If token acquisition fails, do not send the write and show a retryable, user-safe error.

Do not add a CORS workaround or disable antiforgery validation. The Vite proxy is the local-development same-origin boundary.

## Form Behavior and Validation

- Associate every label and error with its input.
- Use correct input types and autocomplete values: `email`, `current-password`, `new-password`, and `name` as appropriate.
- Trim display name and email for validation and submission; never trim or otherwise transform passwords.
- Prevent duplicate submissions while a request is active and communicate the busy state.
- Retain display name, email, and remember-me values after a recoverable failure. Clear password fields when doing so improves safety and may be chosen consistently across both forms.
- Provide client checks for required values, valid email shape, a password of at least 8 characters, and matching password confirmation.
- Treat client validation as feedback only; render server validation because the API remains authoritative.
- On failure, focus the error summary or first invalid field so keyboard and screen-reader users can find the problem.
- Do not reveal whether an account exists during login. Render `invalid_credentials` as a generic email-or-password error.

## API Error Handling

Normalize ASP.NET Core Problem Details and Validation Problem Details into a typed frontend error shape.

- Map validation error keys to their matching fields when possible.
- Map `email_already_registered` to the registration email field and offer navigation to login.
- Map `invalid_credentials` to a generic form-level login error.
- Treat `401` from initial `/me` as anonymous and `401` from login as invalid credentials.
- Treat an unexpected `401` from logout as an already-ended session, clear in-memory auth state, and return to login.
- Show a generic retryable message for network failures, invalid JSON, and unexpected server errors. Do not display raw exception text, response bodies, stack traces, or security values.

## Visual and Accessibility Requirements

- Preserve the project's dark, developer-focused visual direction while making auth forms calm and readable.
- Support a minimum viewport width of 320px without horizontal scrolling.
- Use semantic headings, forms, labels, buttons, and links.
- Provide visible keyboard focus states and sufficient color contrast.
- Errors must not rely on color alone; use text and `aria-describedby`/`aria-invalid` where appropriate.
- Announce form-level status and errors with a suitable live region without creating repeated announcements.
- Respect `prefers-reduced-motion`; authentication does not require decorative animation.

## Suggested Structure

```text
src/devstash-web/src/
├── auth/
│   ├── api.ts
│   ├── AuthContext.tsx
│   ├── auth-types.ts
│   ├── LoginPage.tsx
│   ├── RegisterPage.tsx
│   └── route-guards.tsx
├── app/
│   └── AppShell.tsx
├── test/
│   └── test-utils.tsx
├── App.tsx
└── main.tsx
```

The exact filenames may change if a simpler cohesive structure emerges. Keep authentication-specific code together and avoid introducing a broad architecture hierarchy before the product needs it.

## Testing and Verification

Add a lightweight frontend testing setup compatible with Vite, React 19, and the repository's current TypeScript configuration. Prefer user-facing queries and mock the network boundary; frontend tests must not require a live API or Neon database.

Cover at least:

- Initial `/me` success renders or reaches the protected shell.
- Initial `/me` `401` reaches login without showing an application error.
- An unresolved `/me` request does not flash protected content or redirect prematurely.
- Login fetches a CSRF token, sends it in the required header, updates auth state, and navigates to `/app`.
- Invalid credentials show the generic server error and do not authenticate.
- Registration sends the expected trimmed fields and navigates to login with a success message without authenticating.
- Duplicate email and field validation errors are rendered accessibly.
- Anonymous navigation to `/app` redirects to login.
- Authenticated navigation to login or registration redirects to `/app`.
- Logout obtains a CSRF token, ends the session, clears user state, and navigates to login.
- Network failure during session restoration shows a retry action.

Run and report:

- `npm install` when dependencies change.
- `npm run lint`.
- The new frontend test command.
- `npm run build`.
- Existing `dotnet build --no-restore` and `dotnet test --no-build` to ensure the established backend remains healthy.
- A manual browser smoke test against the proxied local API for registration, login, refresh-based session restoration, protected navigation, and logout.

## Acceptance Criteria

- Anonymous users can register through the existing API and are directed to login after success.
- Registered users can log in and reach `/app` through the secure cookie session.
- Refreshing `/app` restores the session through `/api/auth/me` without reading or persisting the auth cookie in JavaScript.
- Anonymous users cannot see protected shell content, including during session initialization.
- Authenticated users are redirected away from login and registration pages.
- Registration, login, and logout acquire and send a valid antiforgery token.
- Logout ends the server session, clears client auth state, and returns to login.
- Expected API problems produce clear, safe, accessible feedback.
- Network and unexpected server failures have a recoverable UI state.
- The Vite proxy supports relative `/api` calls in development without adding permissive CORS configuration.
- Frontend tests, lint, and production build pass, and existing backend tests continue to pass.

## Out of Scope

- Changes to backend authentication endpoints or the Identity data model
- GitHub or other external OAuth providers
- Email verification
- Forgot-password and reset-password flows
- Two-factor authentication
- Profile editing and password changes
- Role- or plan-based authorization
- Item, collection, tag, search, or dashboard APIs and screens
- A complete production navigation system
- Cross-tab session synchronization
- Production hosting, reverse-proxy configuration, or deployment

The broader product direction in `context/project-overview.md` is background context. Only the behavior and acceptance criteria in this specification define this feature.
