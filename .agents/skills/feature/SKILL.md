---
name: feature
description: Manage DevStash's spec-driven feature lifecycle when explicitly asked to load a feature, start implementation, test the current feature, or complete it safely.
---

# Feature Workflow

Manage one active feature through `context/current-feature.md`.

Interpret the text after `$feature` as an action and optional arguments:

| Invocation | Result |
| --- | --- |
| `$feature load <spec-or-description>` | Prepare the working file without implementing |
| `$feature start` | Explain the approach and implement the loaded feature |
| `$feature test` | Add useful focused tests and run relevant verification |
| `$feature complete` | Review, verify, and safely finish the feature |

If the action is missing or unknown, explain these four options and do not mutate files.

## Working File

Read `context/current-feature.md` completely before every action. Preserve its append-only History section.

The file uses this structure:

- `# Current Feature: <name>` while a feature is active
- `## Status`: `Not Started`, `In Progress`, or `Complete`
- `## Goals`: observable acceptance goals
- `## Notes`: spec path, constraints, and implementation context
- `## History`: completed feature summaries, earliest to latest

Allow only one active feature. Do not replace a `Not Started` or `In Progress` feature without explicit user confirmation.

## Actions

- For `load`, read [references/load.md](references/load.md).
- For `start`, read [references/start.md](references/start.md).
- For `test`, read [references/test.md](references/test.md).
- For `complete`, read [references/complete.md](references/complete.md).

Read only the reference for the requested action, except that `complete` may also read the test reference when verification is required.

## Shared Rules

- Treat feature specs as product requirements. Translate obsolete Next.js-, Prisma-, or Server Action-specific implementation details into ASP.NET Core, EF Core, and React equivalents while preserving user-visible behavior.
- This is a learning project. Before introducing a new .NET or React pattern during implementation, explain the concept and why it fits this feature in plain language.
- Inspect the working tree before mutations. Preserve unrelated or uncertain changes and never assume every local change belongs to the feature.
- Keep implementation proportional to the loaded goals. Report scope expansion instead of silently adding it.
- Use `apply_patch` for hand-authored edits and the appropriate generators only for mechanical scaffolding.
- Never stage with `git add .` and never push, merge, delete branches, or touch production services without the authorization required by the relevant action.
