# DevStash

DevStash is a centralized developer knowledge hub for the code snippets, AI prompts, notes, terminal commands, files, images, links, and project context developers reuse every day.

The goal is to provide one fast, searchable workspace for storing and organizing reusable technical knowledge instead of scattering it across editors, bookmarks, chat history, local folders, and documentation tools.

This repository is a learning-focused rebuild of DevStash using a .NET 10 and ASP.NET Core backend, a React and TypeScript frontend, and PostgreSQL persistence through Entity Framework Core.

## Stack

- .NET 10 and ASP.NET Core Minimal APIs
- React, TypeScript, and Vite
- xUnit for backend tests
- PostgreSQL with Entity Framework Core and ASP.NET Core Identity

## Structure

```text
src/DevStash.Api/          ASP.NET Core API
src/devstash-web/          React frontend
tests/DevStash.Api.Tests/  Backend tests
context/                   Feature specs and progress
```

## Development

```bash
dotnet restore
dotnet build
dotnet test

cd src/devstash-web
npm install
npm run dev
```
