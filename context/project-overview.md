# DevStash Project Overview

## Product Vision

DevStash is a fast, searchable workspace for the technical knowledge developers reuse: code snippets, AI prompts, notes, terminal commands, files, images, links, and project context.

The product replaces knowledge scattered across editors, bookmarks, chat history, shell history, local folders, and documentation tools with one organized hub. This repository is also a learning-focused rebuild in which each feature should make the .NET and React design choices understandable and verifiable.

## Target Users

- Everyday developers who want quick access to reusable snippets, commands, notes, and links.
- AI-first developers who save prompts, context, workflows, and system instructions.
- Educators and content creators who collect code examples and explanations.
- Full-stack developers who organize patterns, boilerplate, and API examples.

## Core Domain

An **item** is the central resource. Every item belongs to one user and one item type, and may belong to multiple collections and have multiple tags.

The built-in item types are:

| Type | Primary value |
| --- | --- |
| Snippet | Reusable source code |
| Prompt | Reusable AI instructions |
| Note | Markdown or plain-text knowledge |
| Command | Terminal commands and scripts |
| File | Uploaded developer resources |
| Image | Uploaded visual resources |
| Link | Bookmarked URLs |

Collections organize related items across types. Tags provide a second, lightweight classification system. Favorites, pins, and recently used timestamps make frequently needed knowledge faster to reach.

## Product Direction

Build the smallest complete path through the product before adding advanced capabilities:

1. Project, database, and authentication foundations.
2. React authentication and protected application navigation.
3. Authenticated item APIs and a usable item workspace.
4. Collections and tags.
5. Search, favorites, pins, and recent items.
6. File/image storage, import, and export.
7. Optional AI assistance and paid-plan capabilities.

Current feature status and completed work are recorded in `context/current-feature.md`. Detailed, implementation-ready boundaries live in `context/features/`.

## Technical Architecture

- ASP.NET Core 10 Minimal APIs provide the backend.
- ASP.NET Core Identity provides user persistence and secure cookie authentication.
- Entity Framework Core with Npgsql persists data in PostgreSQL.
- React 19, TypeScript, and Vite provide the browser application.
- xUnit covers backend behavior; frontend testing is introduced with the first meaningful UI feature.
- The browser and API are same-origin in production. Vite proxies `/api` during local development.

Use DTOs at API boundaries and keep domain entities private to the backend. User-owned data must always be scoped by the authenticated user; client-supplied owner IDs are never authoritative.

## Experience Principles

- Optimize for fast capture and retrieval.
- Use a dark, developer-tool-inspired interface with accessible contrast and visible focus states.
- Keep forms keyboard-friendly and responsive from mobile through desktop.
- Explain errors beside the affected input and preserve safe user input after failures.
- Prefer clear empty, loading, and error states over ambiguous blank screens.
- Build accessible semantics and interaction behavior into each feature rather than postponing them.

## Security and Data Boundaries

- Authentication uses encrypted, secure, HttpOnly cookies; credentials or session tokens are never stored in browser storage.
- Cookie-authenticated write requests require an antiforgery token.
- Secrets and connection strings are never committed.
- Automated tests use isolated persistence unless an explicit development smoke test is requested.
- EF Core owns only the `devstash_dotnet` PostgreSQL schema. The Prisma-managed `public` schema and production resources remain outside this rebuild's development work.

## Deferred Capabilities

GitHub OAuth, email verification, account recovery, two-factor authentication, rate limiting, custom item types, billing, AI features, and production deployment are later features. Their mention in the product direction does not authorize implementing them as part of a narrower feature.
