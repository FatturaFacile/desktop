# AGENTS.md

## Project & Stack Overview
- **Project Name:** FatturaFacile ERP Enterprise (Desktop)
- **Target Framework:** .NET 11 (`net11.0`, `net11.0-windows10.0.19041.0`) with C# preview language version (`LangVersion: preview`, `EnablePreviewFeatures: true`).
- **Architectural Style:** Strict 4-Layer Clean Architecture with **ZERO Tactical DDD Mandate** (pragmatic POCOs and CQRS handlers; strictly NO AggregateRoots, NO internal domain event dispatchers, NO Outbox pattern, NO reflection ValueObjects).
- **Presentation & UI:** WPF (`FatturaFacile.Wpf`), MVVM with `CommunityToolkit.Mvvm`, Syncfusion WPF controls (`FluentDark` theme), zero code-behind in `*.xaml.cs`.
- **Application Orchestration:** MediatR (CQRS commands/queries), FluentValidation (`ValidationBehavior`), AutoMapper, Country Strategy pattern (CH / IT).
- **Persistence & Adapters:** SQLite with Entity Framework Core (`FatturaFacileDbContext`), `TenantSaveChangesInterceptor` for multi-tenancy isolation, Windows Hello biometric auth (`WindowsHelloAuthenticationService`), Syncfusion document export (PDF/Excel).

### Solution Layout
```
FatturaFacile.slnx
├── src/
│   ├── FatturaFacile.Domain/          # Layer 1: Entities (POCOs), Enums, Interfaces, Records (Zero external dependencies)
│   ├── FatturaFacile.Application/     # Layer 2: CQRS Handlers, Behaviors, DTOs, Mappings, Port Interfaces
│   ├── FatturaFacile.Infrastructure/  # Layer 3: SQLite DbContext, EF Core mappings, Biometrics, Export Adapters
│   └── FatturaFacile.Wpf/             # Layer 4: MVVM Views/ViewModels, App Bootstrapper, Shell Window
├── tests/
│   └── FatturaFacile.UnitTests/       # Unit & Handler Tests (xUnit, FluentAssertions, Moq, SQLite in-memory)
├── assets/                            # Brand assets, icons, logos
├── docs/                              # Project documentation & architectural guides
└── Directory.Build.props              # Centralized MSBuild properties, analyzer settings, package references
```

---

## Coding & Quality Standards
- **Strict Typing & Nullability:** `<Nullable>enable</Nullable>` is enforced across all projects.
- **Zero-Warning Tolerance:** `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>` are active. Any compiler or analyzer warning breaks the build.
- **Analyzers Active:** Roslynator, SonarAnalyzer.CSharp, StyleCop.Analyzers, Meziantou.Analyzer, SecurityCodeScan, AsyncFixer, and IDisposableAnalyzers.
- **Layering Boundaries:**
  - `Domain` must NEVER reference external packages or EF Core.
  - `Application` must NEVER reference `Wpf` or `Infrastructure` (unidirectional inward dependencies).
  - `Wpf` views must have zero code-behind logic.
  - ViewModels must use `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`).
- **Multi-Tenancy:** Entities implement `ITenantScopedEntity` or `IOptionalTenantEntity`. `TenantSaveChangesInterceptor` automatically applies tenant scoping.

---

## Exact Verification & Quality Gate Commands
Always execute verification commands before completing any task:

| Step | Command | Description |
| :--- | :--- | :--- |
| **Restore** | `dotnet restore` | Restores all NuGet package dependencies |
| **Build** | `dotnet build --no-restore` | Builds all projects with strict analyzer checks (`TreatWarningsAsErrors`) |
| **Format Check** | `dotnet format --verify-no-changes` | Validates code formatting against `.editorconfig` rules |
| **Format Apply** | `dotnet format` | Automatically formats codebase to adhere to conventions |
| **Run Tests** | `dotnet test --no-build` | Executes unit tests across test projects |
| **All-in-One** | `dotnet build && dotnet test --no-build` | Standard build and test cycle |

---

## Database & Testing Policy
- **Local Persistence & Testing:**
  - FatturaFacile is an offline-first desktop application using local SQLite.
  - Local execution uses SQLite databases.
  - Unit and integration tests run entirely against isolated in-memory SQLite instances or mocked repositories (`Microsoft.Data.Sqlite`, `UseSqlite("DataSource=:memory:")`).
  - Never introduce external SQL server requirements (PostgreSQL, SQL Server) for local testing or desktop runs.
  - Foreign keys must be enabled on SQLite connections (`Foreign Keys=True`).
- **Query Compatibility:**
  - All LINQ queries and EF Core configurations must maintain standard ANSI SQL / SQLite compatibility.
  - Never use server-specific raw SQL dialect syntax.
