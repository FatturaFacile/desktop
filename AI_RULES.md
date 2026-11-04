# FatturaFacile - Instructions & Invariant Rules for AI Coding Assistants

These rules are mandatory for all AI agents (Cursor, Copilot, agy, Claude Code, etc.) interacting with this codebase.

---

## 🚫 Absolute Invariants & Prohibitions

1. **NO UI Dependencies in Domain or Application**:
   - `*.Domain` and `*.Application` projects must **NEVER** reference `PresentationFramework`, `PresentationCore`, `WindowsBase`, WPF, or any UI assemblies.
   - Domain and Application projects must remain pure cross-platform .NET libraries (`TargetFramework: net11.0`).

2. **NO Infrastructure Dependencies in Presentation Layers**:
   - `FatturaFacile.Desktop.Wpf` references `FatturaFacile` (the composition meta-package) or application abstractions, never raw internal infrastructure modules directly.

3. **NO EF Core Dependencies in Domain**:
   - `*.Domain` projects must not reference `Microsoft.EntityFrameworkCore`.
   - Domain entities must not contain EF Core attributes (`[Table]`, `[Key]`, `[ForeignKey]`). Use Fluent API in `*.Infrastructure`.

4. **Multi-Tenancy Integrity**:
   - Every tenant-scoped entity must implement `IMustHaveTenant` or `ITenantEntity` with property `Guid TenantId { get; set; }`.
   - When writing EF Core queries, do not bypass global query filters unless explicitly implementing cross-tenant administrative tasks (`IgnoreQueryFilters()`).
   - Always resolve `TenantId` via `ITenantContext`.

5. **Type Disambiguation**:
   - Always disambiguate WPF types from Windows Forms or base BCL types:
     - Use `System.Windows.Controls.UserControl` (not `System.Windows.Forms.UserControl`).
     - Use `System.Windows.Application.Current` (not `System.Windows.Forms.Application`).

---

## 📁 File Placement Matrix

| Artifact Type | Required Location |
| :--- | :--- |
| Shared Domain Primitives, Aggregates, Value Objects | `src/Common/FatturaFacile.Domain.Shared/` |
| Shared CQRS Interfaces, Behaviors, Localization | `src/Common/FatturaFacile.Application.Shared/` |
| Shared EF Interceptors, Tenancy, Outbox | `src/Common/FatturaFacile.Infrastructure.Common/` |
| Module Domain Aggregates & Repositories | `src/Modules/<Module>/FatturaFacile.<Module>.Domain/` |
| Module CQRS Commands, Handlers & Validators | `src/Modules/<Module>/FatturaFacile.<Module>.Application/` |
| Module EF Core Configurations & Repositories | `src/Modules/<Module>/FatturaFacile.<Module>.Infrastructure/` |
| Module HTTP & Minimal API Endpoints | `src/Modules/<Module>/FatturaFacile.<Module>.Endpoints/` |
| Root Fluent Bootstrapper & Meta-Package | `src/FatturaFacile/` |
| WPF Desktop Shell, MVVM Views & ViewModels | `src/Presentation/FatturaFacile.Desktop.Wpf/` |
| Unit & Integration Tests | `tests/` |

---

## 🛡️ Coding Pattern Expectations

1. **MVVM**:
   - ViewModels inherit from `ViewModelBase` (using `CommunityToolkit.Mvvm`).
   - Use `[ObservableProperty]` and `[RelayCommand]`.

2. **Cross-Border Operations**:
   - Always differentiate between Italian and Swiss requirements:
     - **IT**: SDI FatturaPA 1.2.2, Codice Fiscale, Partita IVA, PEC, 7-character Codice Destinatario.
     - **CH**: Swiss QR-Bill (SCOR 27-digit Modulo 10 reference), UID (`CHE-xxx.xxx.xxx`), QR-IBAN, 0.05 CHF cash rounding.

3. **Dynamic Translations**:
   - Support 4 core languages: Italian (`it`), German (`de`), French (`fr`), and English (`en`).
   - Resolve error messages via `IErrorLocalizer`.

4. **XML Docstrings**:
   - All public types, methods, and properties must include clean English XML documentation.
