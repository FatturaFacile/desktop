# GEMINI.md
@AGENTS.md
- Follow [AGENTS.md](AGENTS.md) as the single source of truth for architecture, coding, localization (`it`, `de`, `fr`, `en`), quality gates, and `dev`-only git workflow.
- Gemini override: verify every change with `dotnet build --configuration Release && dotnet test --configuration Release --no-build` before pushing `origin dev`.
