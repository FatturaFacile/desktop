# JetBrains Junie Guidelines
- Follow [AGENTS.md](../AGENTS.md) as the single source of truth for architecture, coding, localization (`it`, `de`, `fr`, `en`), quality gates, and `dev`-only git workflow.
- Junie override: run `dotnet build --configuration Release && dotnet test --configuration Release --no-build` and `dotnet format --verify-no-changes` before finishing.
