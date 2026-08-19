# Current Feature

## Status

Complete

## Goals

## Notes

## History

Keep this updated from earliest to latest.

- **Foundation Setup** (August 18, 2026)
  - Created the .NET 10 solution with ASP.NET Core API and xUnit projects.
  - Added the React, TypeScript, and Vite frontend.
  - Added a real `/health` endpoint and an integration test using `WebApplicationFactory`.
  - Replaced generated demo screens with a DevStash starter page.
  - Verified NuGet restore, .NET build, xUnit tests, frontend lint, and frontend production build.

- **Database Foundation** (August 19, 2026)
  - Added EF Core 10, Npgsql, and ASP.NET Core Identity persistence with a scoped `DevStashDbContext`, secure configuration, PostgreSQL URI support, and bounded transient retries.
  - Modeled users, item types, items, collections, tags, and explicit join entities with schema-isolated tables, keys, indexes, ownership rules, timestamps, and delete behavior.
  - Added a repository-local `dotnet-ef` tool, generated and reviewed the initial migration and SQL, and applied it only to the configured Neon development branch.
  - Verified 14 tables and seven seeded system item types in `devstash_dotnet`; the Prisma-managed `public` schema and production resources were not changed.
  - Verified restore, a warning-free build, 13 passing tests, reproducible migration SQL, and no pending EF model changes.

- **Authentication Backend Foundation** (August 19, 2026)
  - Loaded the feature specification for secure ASP.NET Core Identity cookie authentication, CSRF protection, and focused API tests.
  - Added registration, login, logout, current-user, and CSRF endpoints with safe DTOs and stable problem codes.
  - Configured secure Identity cookies, explicit password and lockout rules, authentication and authorization middleware, and API-safe `401`/`403` behavior.
  - Added the required display name and unique normalized-email model changes, plus the schema-isolated `AddAuthenticationUserFields` migration and reproducible SQL script.
  - Confirmed `AddAuthenticationUserFields` is applied to the previously verified Neon development database; a targeted EF update reported the database already up to date.
  - Verified a warning-free build, 26 passing tests using isolated SQLite persistence, formatting, a clean diff check, and no pending EF model changes.
