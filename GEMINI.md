# GEMINI.md

@AGENTS.md

## Gemini Execution Instructions
- Refer to [AGENTS.md](file:///C:/dev/fattura-facile/desktop/AGENTS.md) as the single source of truth for solution architecture, coding standards, and commands.
- Verify all changes using:
  ```powershell
  dotnet build
  dotnet test --no-build
  ```
- Maintain strict nullability and analyzer standards across all .NET 11 projects.
