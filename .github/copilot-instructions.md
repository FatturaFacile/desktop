# GitHub Copilot Instructions

See [AGENTS.md](../AGENTS.md) for full project architecture and conventions.

## Key Stack Constraints
- **Framework:** .NET 11 (`net11.0` / `net11.0-windows10.0.19041.0`) with C# preview features enabled.
- **Architecture:** 4-Layer Clean Architecture (`Domain` <- `Application` <- `Infrastructure` / `Wpf`).
- **Core Design Rule:** ZERO Tactical DDD. Use pragmatic POCOs, MediatR CQRS handlers, and FluentValidation. Never introduce AggregateRoots or domain event pipelines.
- **WPF / MVVM:** Zero code-behind in `*.xaml.cs`. Use `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`) in ViewModels.
- **Database:** Local SQLite (`FatturaFacileDbContext`) with `TenantSaveChangesInterceptor`. Test with in-memory SQLite (`DataSource=:memory:`).

## Verification Commands
```powershell
dotnet build
dotnet test --no-build
```
