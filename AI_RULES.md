# FatturaFacile - Instructions & Invariant Rules for AI Coding Assistants

These rules are mandatory for all AI agents (Cursor, Copilot, agy, Claude Code, etc.) interacting with this codebase.

---

## 🚫 Absolute Invariants & Prohibitions

1. **NO UI Dependencies in Domain or Application**:
   - `FatturaFacile.Domain` and `FatturaFacile.Application` must **NEVER** reference `PresentationFramework`, `PresentationCore`, `WindowsBase`, or any WPF/UI assemblies.
   - Domain and Application projects must remain pure cross-platform .NET libraries (`TargetFramework: net11.0` or `net9.0`).

2. **NO EF Core Dependencies in Domain**:
   - `FatturaFacile.Domain` must not reference `Microsoft.EntityFrameworkCore`.
   - Domain entities must not contain EF Core attributes (`[Table]`, `[Key]`, `[ForeignKey]`). Use Fluent API in `FatturaFacile.Infrastructure`.

3. **WPF Client Project Name**:
   - The WPF presentation project is named strictly **`FatturaFacile`** (located at `src/FatturaFacile/`).
   - Do **NOT** rename it to `ERP.UI.Wpf` or `FatturaFacile.Wpf`.

4. **Multi-Tenancy Integrity**:
   - Every tenant-aware domain entity must implement `ITenantEntity` with property `Guid TenantId { get; set; }`.
   - When writing EF Core queries, do not bypass global query filters unless explicitly implementing cross-tenant administrative tasks.
   - Always resolve `TenantId` via `ITenantProvider` rather than hardcoding.

5. **No Stubbed Domain Models**:
   - Do not replace rich business logic with stub comments such as `"// TODO: Add properties"`. All business models must contain realistic fields, validation checks, and domain methods.

---

## 📁 File Placement Matrix

| Artifact Type | Required Location |
| :--- | :--- |
| Rich Entities, Value Objects, Enums | `src/FatturaFacile.Domain/` (`Entities/`, `ValueObjects/`, `Enums/`) |
| Domain Events, Repository Interfaces | `src/FatturaFacile.Domain/` (`Events/`, `Repositories/`) |
| MediatR Commands, Queries, Handlers | `src/FatturaFacile.Application/Features/<FeatureName>/` |
| AutoMapper Profiles | `src/FatturaFacile.Application/Mappings/` |
| DTOs | `src/FatturaFacile.Application/DTOs/` |
| EF Core Configurations & Repositories | `src/FatturaFacile.Infrastructure/Persistence/` |
| Exporters & Cross-Border Parsers | `src/FatturaFacile.Infrastructure/CrossBorder/` |
| WPF Views & ViewModels | `src/FatturaFacile/Views/` & `src/FatturaFacile/ViewModels/` |
| Dynamic XAML Themes & Extensions | `src/FatturaFacile/Themes/` & `src/FatturaFacile/Markup/` |
| Unit & Integration Tests | `tests/FatturaFacile.Application.UnitTests/` |

---

## 🛡️ Coding Pattern Expectations

1. **MVVM**:
   - Use `CommunityToolkit.Mvvm` attributes (`[ObservableProperty]`, `[RelayCommand]`).
   - For exception handling in ViewModels, catch `Exception` and set `ErrorMessage = ex.Message;` while logging via `ILogger`.
   - Disambiguate WPF `Application` references with `System.Windows.Application.Current`.

2. **Cross-Border Operations**:
   - Always differentiate between Italian and Swiss requirements:
     - IT: SDI FatturaPA XML, Codice Fiscale, Partita IVA, PEC, 7-character Codice Destinatario, D.Lgs. 66/2003 rest intervals.
     - CH: Swiss QR-Bill (SCOR 27-digit reference), UID (`CHE-xxx.xxx.xxx`), QR-IBAN (IID 30000-31999), 0.05 CHF cash rounding, ArG / OLL 1 weekly limits.

3. **Dynamic Translations**:
   - Never hardcode user-facing strings in XAML. Always use `{loc:Translate KeyName, Default='Fallback'}`.
   - Seed new translation keys in `DbInitializer.cs` across all 4 locales (`it`, `de`, `fr`, `en`).
