# Load

Prepare a feature for later implementation.

1. Parse the text after `load`:
   - For a path or slug, look for the exact path, then `context/features/<slug>.md`.
   - For a multi-word description, treat it as an inline feature request and derive a concise feature name and observable goals.
   - With no argument, list available specs and summarize the current feature; do not mutate files.
2. Read the selected spec completely. If it references related requirements, read only those needed to understand its acceptance criteria.
3. If another feature is `Not Started` or `In Progress`, report it and ask before replacing it.
4. Update `context/current-feature.md`:
   - Set the heading to `# Current Feature: <name>`.
   - Set Status to `Not Started`.
   - Write measurable bullets under Goals.
   - Record the source spec and relevant constraints under Notes.
   - Preserve History exactly.
5. Report the loaded feature, its goals, important decisions, and any Next.js-to-.NET adaptations.

Do not create a branch or implement code during `load`.
