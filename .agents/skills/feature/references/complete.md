# Complete

Safely finish the current feature. Completion includes review and verification; it does not imply permission to publish remotely.

## Preflight

1. Read `context/current-feature.md` and the source spec.
2. Compare every Goal with the implementation and identify missing behavior, defects, or scope creep.
3. Inspect `git status --short`, the current branch, and the complete feature diff.
4. Run the verification defined in [test.md](test.md). Fix only issues within the loaded feature's scope.
5. If Goals or tests are incomplete, report the gaps and stop without marking Complete.
6. Identify feature files, unrelated or uncertain files, a proposed commit message, and the proposed target branch (normally `main`).
7. Ask for explicit approval before staging, committing, switching branches, or merging.

## Local Completion

Only after approval:

1. Update `context/current-feature.md`:
   - Reset the heading to `# Current Feature`.
   - Set Status to `Complete`.
   - Clear Goals and Notes while retaining their headings.
   - Append a dated summary and verification evidence to History.
2. Stage only the approved feature files. Never use `git add .`.
3. Commit with the approved message.
4. Switch to the approved target branch and merge the feature branch.
5. Report the resulting branch, commits, merge result, and working-tree status.

Stop before pushing or deleting any branch. Remote publication and branch deletion require a separate explicit approval that names the exact operations.

If a merge conflict occurs, report the conflict files and stop for direction. Never discard user changes to resolve it automatically.
