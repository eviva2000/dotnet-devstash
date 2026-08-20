# Authenticated Item Workspace

## Goal

Turn the authenticated shell into the first useful DevStash workspace. Authenticated users can list, create, view, edit, favorite, pin, mark as used, and delete their own text and link items through a small ASP.NET Core API and a React workspace.

This feature establishes the item API contract and the complete first-item workflow. Collections, tags, full-text search, uploads, imports, and dashboard analytics remain later features.

## Learning Outcomes

By completing this feature, we should understand:

- How Minimal API route groups and endpoint filters keep a resource API cohesive.
- How claims establish the server-side owner boundary for every query and mutation.
- How EF Core projections produce safe DTOs without serializing tracked domain entities.
- How to model create, update, partial state-change, and delete operations with clear HTTP contracts.
- How a React workspace coordinates loading, empty, error, and mutation states against a real API.
- How optimistic-looking interactions can remain correct by refetching or reconciling authoritative server responses.
- How keyboard-friendly editing and destructive-action confirmation work in a responsive application UI.

## Existing Foundation

Reuse the existing:

- ASP.NET Core 10 Minimal API project and `DevStashDbContext`.
- `Item`, `ItemType`, and `ApplicationUser` entities and their established `devstash_dotnet` schema.
- Seven seeded system item types: `snippet`, `prompt`, `note`, `command`, `file`, `image`, and `link`.
- Cookie authentication, authorization middleware, safe current-user claims, and `GET /api/auth/me`.
- Antiforgery contract: obtain `GET /api/auth/csrf` immediately before each cookie-authenticated write and send `X-XSRF-TOKEN`.
- React auth provider, protected `/app` route, Vite `/api` proxy, typed auth client patterns, Vitest, and Testing Library.

Do not change Identity, cookie settings, database schema isolation, or the existing authentication endpoint contract. Do not trust a user ID supplied by a client; derive ownership exclusively from the authenticated principal.

## Scope

Deliver:

- Authenticated item-type discovery for the supported built-in types.
- User-scoped item list, detail, create, update, state-change, and delete API endpoints.
- A protected `/app` workspace with a useful item list, filters, empty/error/loading states, and an item editor.
- Creation and editing of `snippet`, `prompt`, `note`, `command`, and `link` items.
- Favorite, pin, and “mark used” actions from the workspace.
- Focused API and frontend tests for ownership, validation, antiforgery, and user-visible workflows.

The browser must call only relative `/api/...` URLs and must never send `userId`, write directly to the database, or persist item data as a substitute for the API.

## Supported Item Types and Content Rules

The API returns all seeded system item types so clients can render recognizable metadata. In this feature, only these types may be created or edited:

| Slug | Required fields | Optional fields | Semantics |
| --- | --- | --- | --- |
| `snippet` | title, content | description, language | Source code or configuration text. |
| `prompt` | title, content | description | Reusable AI instruction text. |
| `note` | title, content | description | Markdown or plain-text knowledge. |
| `command` | title, content | description, language | A command or script; language is a descriptive label such as `bash` or `powershell`. |
| `link` | title, url | description | A valid absolute HTTP or HTTPS URL. |

`file` and `image` are visible as future types but cannot be created through this feature. Return a client-safe validation problem if a caller attempts it; do not create placeholder upload records.

For every supported type:

- Trim title, description, language, and URL before validation and persistence; convert a blank optional value to `null`.
- Never transform item content. Preserve its whitespace exactly.
- Require a nonblank title of at most 200 characters and an optional description of at most 2,000 characters.
- Limit language to 100 characters and URL to 2,048 characters, matching the established persistence limits.
- Require nonblank content for text types and reject content for a link unless a future feature explicitly introduces a link note/body.
- Require a valid absolute `http` or `https` URL for a link and reject URL for text types.

## API Design

Place this resource behind an authorized `/api/items` route group. Give endpoints stable route names. All responses are DTOs; `Item`, `ItemType`, join entities, user objects, and EF navigation properties never cross the API boundary.

### Item Type Discovery

`GET /api/item-types`

Requires authentication. Return the seeded system types ordered consistently by their intended product order. Each response includes `id`, `name`, `slug`, `icon`, and `color`. It must not expose custom types because custom item types are outside this feature.

### List Items

`GET /api/items`

Requires authentication. Return only items whose `UserId` is the current user.

Support these bounded query parameters:

- `type`: optional supported item-type slug filter.
- `favorite`: optional `true` filter.
- `pinned`: optional `true` filter.
- `page`: one-based, default `1`.
- `pageSize`: default `25`, maximum `100`.

Order items deterministically: pinned items first, then most recently updated, then ID as a stable tie-breaker. Return a paged DTO with `items`, `page`, `pageSize`, and `totalCount`. List cards include enough safe data to render a preview, but truncate content server-side to a documented bounded preview rather than returning unbounded bodies for every row.

Malformed filters or paging values return a validation problem. A valid type that has no items returns an empty page, not `404`.

### Read an Item

`GET /api/items/{itemId}`

Requires authentication. Fetch through an owner-scoped query. Return `404 Not Found` for a missing item and for another user’s item so ownership is not disclosed. The detail DTO includes the complete supported content or URL plus timestamps and state flags.

### Create an Item

`POST /api/items`

Requires authentication and a valid antiforgery token. Accept a create DTO containing `type`, `title`, `description`, `content`, `language`, and `url`; do not accept owner, timestamps, favorite, pin, or last-used fields. Resolve the seeded item type by slug server-side, validate type-specific rules, set the authenticated user as owner, and return `201 Created` with the detail DTO and a `Location` header for the new resource.

### Update an Item

`PUT /api/items/{itemId}`

Requires authentication and a valid antiforgery token. Accept the same editable fields as creation and replace those fields after type-specific validation. Type is immutable for this feature: do not accept it on update and do not implement conversion between item kinds. Set `UpdatedAt` in UTC only when an update succeeds. Missing or foreign items return `404`.

### Item State Actions

Use narrow, explicit endpoints for lightweight state changes rather than accepting arbitrary entity patches:

- `PUT /api/items/{itemId}/favorite` with `{ "isFavorite": true | false }`
- `PUT /api/items/{itemId}/pinned` with `{ "isPinned": true | false }`
- `POST /api/items/{itemId}/used`

All require authentication and a valid antiforgery token, apply owner-scoped lookup, return the updated safe detail DTO, and set `UpdatedAt`. The `used` endpoint sets `LastUsedAt` to the server’s current UTC time; clients must not submit a timestamp.

### Delete an Item

`DELETE /api/items/{itemId}`

Requires authentication and a valid antiforgery token. Delete only through an owner-scoped lookup. Return `204 No Content`; return `404` for absent or foreign IDs. EF relationship configuration must remove the item’s existing join rows, without affecting collections or tags themselves.

### Errors and Safety

- Reuse ASP.NET Core Problem Details / Validation Problem Details and stable, client-safe problem codes where the current API has established that practice.
- API validation errors must be field-addressable where possible; unexpected errors must not expose exception text, SQL, claims, CSRF values, cookies, or connection details.
- `401` and `403` remain API responses, never HTML redirects.
- Ensure antiforgery rejection has a consistent client-safe response.
- Use `AsNoTracking()` for read projections where appropriate. Avoid N+1 query behavior when projecting item-type information.

## React Workspace

Keep `/app` protected by the existing auth guard. Replace the authentication-proof content with a workspace that is helpful even when no data exists.

### Layout and Navigation

- Keep the DevStash brand, signed-in context, and logout action from the existing shell.
- Add an obvious “New item” action that opens the editor on desktop and mobile.
- Render a compact type filter, plus favorite and pinned filters. Filtering changes the fetched list and resets to page one.
- Show a paginated item list with type, title, concise preview, state indicators, and updated time. Do not require collection or tag navigation.
- Selecting an item loads its detail into the editor. Preserve the selected item until a clear replacement, deletion, or route transition.
- Use a simple route or in-workspace state arrangement consistent with the current router. A shareable deep-link item route is optional and must not delay the core workflow.

### Editor Behavior

- The new-item editor begins by selecting one of the five supported types; file and image are visibly unavailable with a short explanation.
- Render only fields meaningful to the selected type. Content uses a labeled textarea; do not add syntax highlighting, Markdown preview, rich text, code execution, URL unfurling, or file pickers.
- Edit mode preserves the immutable type and allows updating title, description, content/language, or URL as applicable.
- Validate required fields and basic URL shape in the client for immediate feedback, then render server validation as authoritative feedback.
- Disable duplicate submissions and announce saving state. Retain safe input after a recoverable failure; do not silently discard content.
- On create, add the returned item to the current workspace state or refetch, select it, and show a clear success status. On update, reconcile the returned item. Do not invent client-generated IDs.
- Provide favorite, pin, and mark-used controls for a selected item. Handle failures safely and restore or refetch state so the UI does not claim a mutation succeeded when it did not.
- Deletion requires an explicit confirmation step that names the item. On success, remove it from the list, clear the editor selection, and place sensible keyboard focus on the next useful control.

### States and Accessibility

- Show an intentional loading state during initial list load; do not render a misleading empty state first.
- Show an item-specific empty state with a New item action when the user has no items or no items match the active filters.
- Show retryable, user-safe errors for list/detail/mutation network failures and unexpected API responses. Do not show raw response bodies.
- Use semantic headings, landmarks, buttons, labels, and form controls. Associate client and server errors with their fields using `aria-describedby` and `aria-invalid`.
- Move focus to the error summary or first invalid field after submission failure; use a restrained live region for operation status.
- Ensure all controls work by keyboard, visible focus indicators remain clear, and the layout works at 320px without horizontal scrolling.
- Maintain the existing dark, developer-tool visual direction and respect `prefers-reduced-motion`.

## Suggested Structure

```text
src/DevStash.Api/
├── Features/
│   └── Items/
│       ├── Contracts/
│       ├── ItemEndpoints.cs
│       └── ItemMappings.cs
└── Features/
    └── ItemTypes/
        └── ItemTypeEndpoints.cs

src/devstash-web/src/
├── items/
│   ├── api.ts
│   ├── item-types.ts
│   ├── ItemWorkspace.tsx
│   ├── ItemList.tsx
│   ├── ItemEditor.tsx
│   └── *.test.tsx
└── app/
    └── AppShell.tsx
```

Exact filenames may vary. Keep API contracts, mapping, and validation close to the Items feature, and keep browser fetch/error normalization in a focused item client rather than distributing it across components.

## Testing and Verification

Add focused backend tests using isolated persistence and authenticated test users. Cover at least:

- Anonymous requests are rejected for every item and item-type endpoint.
- A user can create each supported item kind and receives only safe DTO fields.
- Type-specific validation rejects missing content, invalid links, unsupported file/image creation, invalid lengths, and attempts to set owner or state fields.
- List paging/filtering/order is deterministic and scoped to the current user.
- A user cannot read, update, state-change, or delete another user’s item; each returns `404` where specified.
- Create, update, state actions, and delete require a valid antiforgery token.
- `used` uses a server-issued UTC timestamp and update actions return authoritative state.
- Delete removes only the owned item and its joins, not unrelated items, collections, or tags.

Add frontend tests that mock the API boundary and cover at least:

- Initial loading, empty, and retryable list-error states.
- Creating a snippet obtains CSRF, submits expected values, and renders/selects the returned item.
- Creating a link renders URL-specific validation and server field errors accessibly.
- Selecting and editing an item sends a valid update and reconciles the response.
- Favorite, pin, and mark-used actions include a CSRF token and display failure safely.
- Deletion requires confirmation and removes the item after success.
- Filters change the requested list and an empty filtered result is understandable.
- The protected workspace does not request item data before auth initialization has resolved.

Run and report:

- `dotnet build --no-restore`
- `dotnet test --no-build`
- `npm run lint`
- the existing frontend test command
- `npm run build`
- a manual browser smoke test through the Vite proxy: create a text item and link, refresh, edit, favorite/pin, mark used, filter, and delete.

## Acceptance Criteria

- An authenticated user can create, read, update, favorite, pin, mark used, and delete only their own supported items.
- Anonymous and cross-user access is safely rejected, with foreign item IDs behaving as not found.
- The API uses DTOs, owner-scoped queries, stable validation behavior, and antiforgery protection for every write.
- The workspace provides an accessible, responsive first-item workflow with clear loading, empty, error, success, and confirmation states.
- Snippet, prompt, note, command, and link semantics are validated consistently by client feedback and authoritative API validation.
- File and image types are not implemented as fake uploads or placeholder resources.
- Backend and frontend tests pass without requiring Neon, and the existing authentication flow continues to work.

## Out of Scope

- Collection and tag CRUD, assignment, filtering, or navigation
- Search, favorites/pins dashboards, recent-items views, sorting controls beyond the defined list order, or analytics
- File or image upload, storage metadata, previews, downloads, or deletion from object storage
- Import/export, share links, collaboration, and public items
- Markdown preview, syntax highlighting, command execution, URL metadata fetches, or browser extensions
- Custom item types and user-created type configuration
- Changes to authentication, profiles, password management, roles, billing, AI features, deployment, or the Prisma-managed `public` schema

The broader product direction in `context/project-overview.md` is background context. Only the behavior and acceptance criteria in this specification define this feature.
