# DevStash — .NET + React

A learning-focused rebuild of DevStash with an ASP.NET Core backend and a React frontend.

## Stack

- .NET 10 and ASP.NET Core Minimal APIs
- React, TypeScript, and Vite
- xUnit for backend tests
- PostgreSQL with Entity Framework Core in the next milestone

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
