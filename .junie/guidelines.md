# JetBrains AI Assistant & Junie Guidelines

This project uses standard conventions defined in [AGENTS.md](../AGENTS.md).

## Technology Stack & Guidelines
- **Target:** .NET 11 preview C#, WPF, MVVM (CommunityToolkit.Mvvm), SQLite EF Core.
- **Architectural Rules:** Clean Architecture with unidirectional dependencies (`Domain` <- `Application` <- `Infrastructure` / `Wpf`). ZERO Tactical DDD (POCOs + MediatR CQRS only).
- **Code Style:** `<Nullable>enable</Nullable>`, zero warnings tolerated (`TreatWarningsAsErrors=true`).

## Verification Commands
- **Build Solution:** `dotnet build`
- **Execute Tests:** `dotnet test --no-build`
- **Code Formatting:** `dotnet format`
