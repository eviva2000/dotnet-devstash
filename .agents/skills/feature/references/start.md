# Start

Begin the feature currently loaded in `context/current-feature.md`.

1. Verify that the feature has a name, Status is `Not Started`, and Goals are populated. Otherwise report what must be loaded or corrected.
2. Inspect `git status --short` and the current branch. Identify unrelated or uncertain changes before proceeding.
3. Explain the relevant .NET, ASP.NET Core, EF Core, React, or testing concepts and summarize the intended data/control flow before editing.
4. Create a concise implementation plan mapped to the Goals.
5. When starting from `main`, create a `codex/<feature-slug>` branch if the working tree can be carried safely. If existing changes make branch ownership unclear, stop and ask for direction.
6. Set Status to `In Progress`.
7. Implement the Goals one by one, keeping backend contracts and frontend behavior aligned.
8. Run focused verification during implementation and report material deviations from the spec.

Do not commit, merge, push, or mark the feature Complete during `start`.
