# Test

Verify the current feature according to its Goals and changed files.

1. Read `context/current-feature.md`, the source spec, and the feature diff.
2. Map each Goal to observable verification. Identify meaningful untested logic before adding tests.
3. For backend changes:
   - Add focused xUnit unit tests for business logic.
   - Add `WebApplicationFactory` integration tests for HTTP contracts when useful.
   - Run `dotnet restore` only when dependencies changed or assets are missing.
   - Run `dotnet build --no-restore` and `dotnet test --no-build` after a successful build.
4. For frontend changes:
   - Run `npm install` only when dependencies changed or are missing.
   - Run `npm run lint` and `npm run build` when those scripts exist.
   - Run the frontend test script when one exists; do not add a test framework merely to satisfy the workflow.
5. For EF Core migrations or database changes, inspect generated migration SQL and verify only the configured development target. Never use production by default.
6. Report every command, its result, Goals covered, remaining gaps, and whether the feature is ready for completion.

Do not change feature status, commit, merge, or push during `test`.
