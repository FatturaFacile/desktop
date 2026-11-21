# AGENTS.md — FatturaFacile Desktop (.NET 11 / WPF / SQLite / Syncfusion)

## Stack & Clean Architecture
- Solution: `FatturaFacile.slnx` | SDK: .NET 11 (`global.json`, `LangVersion=preview`, `Nullable=enable`, `TreatWarningsAsErrors=true`, `AnalysisMode=All`).
- Strict inward dependencies (`Domain <- Application <- Infrastructure / Wpf`):
  - `src/FatturaFacile.Domain` (`net11.0`): zero external packages; POCO entities, enums, `Money` record, `IBaseEntity`, `ITenantScopedEntity`, `IOptionalTenantEntity`.
  - `src/FatturaFacile.Application` (`net11.0`): depends ONLY on `Domain`; MediatR CQRS (`ValidationBehavior`, `AuthorizationBehavior`), FluentValidation, AutoMapper (`MappingProfile`), country strategies, port interfaces; zero SQL/EF/WPF refs.
  - `src/FatturaFacile.Infrastructure` (`net11.0-windows10.0.19041.0`): EF Core SQLite (`FatturaFacileDbContext`, `TenantSaveChangesInterceptor`), `Repository<T>`/`IUnitOfWork`, Windows Hello, AES-256-GCM + DPAPI, Syncfusion export, QRCoder, sync/copilot/DSS adapters.
  - `src/FatturaFacile.Wpf` (`net11.0-windows10.0.19041.0`): pure MVVM (`CommunityToolkit.Mvvm`), Syncfusion WPF (`FluentDark`, `ChromelessWindow`, `SfDataGrid`), `Microsoft.Extensions.Hosting`, `.resx` i18n.
  - `tests/FatturaFacile.UnitTests` (`net11.0-windows10.0.19041.0`): xUnit, Moq, EF Core InMemory, Coverlet.

## ZERO Tactical DDD Mandate
- FORBIDDEN: `AggregateRoot`, `Entity<TId>`, domain events/dispatchers, outbox tables/workers, reflection `ValueObject` classes, private setters/constructors on entities.
- REQUIRED: plain POCO entities with public parameterless constructor and `{ get; set; }` properties; composite values as `sealed record` (e.g., `Money`); business rules/orchestration strictly in MediatR handlers and strategies.

## Coding Standards & Analyzers (Zero Warnings)
- `Directory.Build.props` + `.editorconfig` enforce `TreatWarningsAsErrors=true` across `NetAnalyzers`, `StyleCop`, `SonarAnalyzer`, `Meziantou`, `Roslynator`, `SecurityCodeScan`, `AsyncFixer`, `IDisposableAnalyzers`, `Microsoft.Extensions.StaticAnalysis`.
- Copyright header (`SA1633`, `assets/stylecop.json` company `Fattura Facile`) required at top of every `.cs` file:
  - `// <copyright file="<ExactFileName>.cs" company="Fattura Facile">` / `// Copyright (c) Fattura Facile. All rights reserved.` / `// </copyright>`
- One top-level type per `.cs` file (`SA1402`): separate `*Command.cs`, `*CommandHandler.cs`, `*CommandValidator.cs`, `*Query.cs`, `*Dto.cs`.
- English XML docs (`CS1591`/`SA1600`) required on all public types and members (`<summary>`, `<param>`, `<returns>`).
- Syntax/naming (`.editorconfig`): file-scoped namespaces, `using` outside namespace, `var`, primary constructors, collection expressions (`[...]`), explicit accessibility, `sealed` internal classes (`CA1852`), forward `CancellationToken` (`CA2016`), CRLF/UTF-8/4-space; `_camelCase` private fields, `s_camelCase` static readonly, `ALL_UPPER` consts, `IPascalCase` interfaces.
- Update `src/FatturaFacile.Application/Common/Mappings/MappingProfile.cs` deterministically for new DTOs/entities and verify with `AutoMapperConfigurationTests`.

## Pure MVVM & WPF
- Zero code-behind: `*.xaml.cs` contains ONLY `InitializeComponent()` (only `App.xaml.cs` bootstraps host/auth and `MainWindow.xaml.cs` binds DataContext). Never add UI event handlers in code-behind.
- ViewModels inherit `ObservableObject`, use `[ObservableProperty]` on private `_fields` (not partial properties; `MVVMTK0042=none`), `[RelayCommand]` for actions, and `ISender` for MediatR.
- Apply `sfSkinManager:SfSkinManager.VisualStyle="FluentDark"`; use XAML bindings and `Microsoft.Xaml.Behaviors.Wpf`.

## Localization (it, de, fr, en) & Country Strategies (CH, IT)
- Supported cultures (`LocalizationService`): `it-IT` (default fallback), `en-US`, `de-CH`, `fr-CH`.
- Update ALL 4 resource files together in `src/FatturaFacile.Wpf/Localization/`: `Strings.resx` (`it`), `Strings.de.resx` (`de`), `Strings.fr.resx` (`fr`), `Strings.en.resx` (`en`). Never hardcode user-facing text (`CA1303`); verify with `LocalizationTests`.
- Regional rules live in `ICountryStrategyFactory` (never `if (isSwiss)` checks in entities/views):
  - `CH`: `CHF`, 0.05 cash rounding (`Math.Round(x * 20m) / 20m`), QR-bill SPC 0200 (`SwissQrBillPayloadBuilder`, structured addresses, QRR w/ QR-IBAN or SCOR), Swiss payroll preview (not Swissdec-certified; no ELM).
  - `IT`: `EUR`, 0.01 `AwayFromZero` rounding, FatturaPA v1.2 XML (`FatturaPaXmlBuilder`, FPR12/FPA12, split payment, reverse charge, ritenuta, bollo > €77.47), Italian payroll preview.
- Never overstate regulatory certification; check [COMPLIANCE.md](COMPLIANCE.md).

## Multi-Tenancy, SQLite & Security
- Tenant-private entities: `ITenantScopedEntity` (`Guid TenantId`); shared defaults: `IOptionalTenantEntity` (`Guid? TenantId`, `null` = global read-only); timestamps: `IBaseEntity`.
- Never bypass `TenantSaveChangesInterceptor` (blocks modifying/deleting `TenantId == null` with `InvalidOperationException`, auto-sets `TenantId`, stamps UTC `CreatedAt`/`UpdatedAt`). Verify with `MultiTenantSecurityTests`.
- SQLite offline-first (`fattura-facile.sqlite`, WAL, FK on). New EF migration:
  - `dotnet tool restore && dotnet ef migrations add <Name> --project src/FatturaFacile.Infrastructure --startup-project src/FatturaFacile.Wpf --output-dir Persistence/Migrations`
- Security: Windows Hello (`UserConsentVerifier`) fail-closed in Release (Debug fallback only); AES-256-GCM (`AesGcmFieldEncryptor`) + DPAPI for PII; PBKDF2 password hashing; no weak crypto/randomness (`CA5350`–`CA5403`).

## Quality Gates
- Mandatory before any commit/push (zero warnings, zero errors):
  - `dotnet build --configuration Release`
  - `dotnet test --configuration Release --no-build`
- Format check/fix: `dotnet format` / `dotnet format --verify-no-changes`.
- NEVER push to `origin dev` unless all quality gates pass with zero warnings/errors.

## Strict Git Workflow (dev Only, ZERO PRs, ZERO Leftovers)
- Integrate directly into `dev` via local merge from primary repo root:
  - `git checkout dev && git pull --rebase origin dev && git merge --no-ff <branch>`
- FORBIDDEN: NEVER commit, merge, or push to `main`. NEVER create Pull Requests (`gh pr create` / `gh pr`).
- Conflict resolution: resolve all merge conflicts autonomously preserving Clean Architecture, strict types, `TreatWarningsAsErrors`, and all 4 locales (`it`, `de`, `fr`, `en`).
- Mandatory end-of-task cleanup after pushing `origin dev`:
  - Delete merged local/remote feature branches: `git branch -D <branch>` and `git push origin --delete <branch>`.
  - Remove any linked worktree: `git worktree remove --force <path> && git worktree prune`.
  - Delete all temporary/debug files so `git status` is completely clean.
- Reference docs: [ARCHITECTURE.md](ARCHITECTURE.md) | [CONTRIBUTING.md](CONTRIBUTING.md) | [COMPLIANCE.md](COMPLIANCE.md).
