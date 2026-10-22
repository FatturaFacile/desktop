# FatturaFacile - Instructions & Invariant Rules for AI Coding Assistants

These rules are mandatory for all AI agents (Cursor, Copilot, agy, Claude Code, etc.) interacting with this codebase.

---

## 🚫 Absolute Invariants & Prohibitions

1. **NO UI Dependencies in Domain or Application**:
   - `FatturaFacile.Domain` and `FatturaFacile.Application` must **NEVER** reference `PresentationFramework`, `PresentationCore`, `WindowsBase`, DevExpress WPF, or any UI assemblies.
   - Domain and Application projects must remain pure cross-platform .NET libraries (`TargetFramework: net11.0`).

2. **NO Infrastructure Dependencies in Presentation Modules**:
   - Presentation modules (`FatturaFacile.Wpf.Modules`, `FatturaFacile.Wpf.Reporting`, `FatturaFacile.Wpf.AI`) must **NEVER** reference `FatturaFacile.Infrastructure`.
   - Presentation modules depend exclusively on `FatturaFacile.Application` and `FatturaFacile.Wpf.Common`.
   - Only the host shell ([FatturaFacile](file:///C:/dev/fattura-facile/desktop/src/FatturaFacile/FatturaFacile.csproj)) references `FatturaFacile.Infrastructure` as the composition root.

3. **NO EF Core Dependencies in Domain**:
   - `FatturaFacile.Domain` must not reference `Microsoft.EntityFrameworkCore`.
   - Domain entities must not contain EF Core attributes (`[Table]`, `[Key]`, `[ForeignKey]`). Use Fluent API in `FatturaFacile.Infrastructure`.

4. **Multi-Tenancy Integrity**:
   - Every tenant-aware domain entity must implement `ITenantEntity` with property `Guid TenantId { get; set; }`.
   - When writing EF Core queries, do not bypass global query filters unless explicitly implementing cross-tenant administrative tasks (`IgnoreQueryFilters()`).
   - Always resolve `TenantId` via `ITenantProvider` rather than hardcoding.

5. **Windows Store (MSIX) Read-Only Directory Constraint**:
   - Never write files, databases, or logs to `AppDomain.CurrentDomain.BaseDirectory`.
   - Always persist mutable state (`FatturaFacile.db`, `userstate.json`) to `%LocalAppData%\FatturaFacile\`.

6. **Type Disambiguation**:
   - Always disambiguate WPF types from Windows Forms or base BCL types:
     - Use `System.Windows.Controls.UserControl` (not `System.Windows.Forms.UserControl`).
     - Use `System.Windows.Application.Current` (not `System.Windows.Forms.Application`).

---

## 📁 File Placement Matrix

| Artifact Type | Required Location |
| :--- | :--- |
| Rich Entities, Value Objects, Enums | `src/FatturaFacile.Domain/` (`Entities/`, `ValueObjects/`, `Enums/`) |
| Domain Events, Repository Interfaces | `src/FatturaFacile.Domain/` (`Events/`, `Repositories/`) |
| MediatR Commands, Queries, Handlers, Validators | `src/FatturaFacile.Application/Features/<FeatureName>/` |
| AutoMapper Profiles | `src/FatturaFacile.Application/Mappings/` |
| DTOs & Application Interfaces | `src/FatturaFacile.Application/DTOs/` & `Interfaces/` |
| EF Core Configurations & DbContext | `src/FatturaFacile.Infrastructure/Persistence/` |
| Cross-Border Parsers & Exporters | `src/FatturaFacile.Infrastructure/CrossBorder/` |
| Shared UI Contracts, Converters, Base ViewModels | `src/FatturaFacile.Wpf.Common/` |
| Business Feature Views & ViewModels | `src/FatturaFacile.Wpf.Modules/Views/` & `ViewModels/` |
| DevExpress Reports, Viewers & Designers | `src/FatturaFacile.Wpf.Reporting/` |
| AI Assistants, Chat UI, Semantic Kernel Tools | `src/FatturaFacile.Wpf.AI/` |
| Shell Window, Host Builder, Composition Root | `src/FatturaFacile/` (`App.xaml.cs`, `MainWindow.xaml`) |
| Domain & Application Unit Tests (xUnit) | `tests/FatturaFacile.Application.UnitTests/` |
| WPF Navigation & Integration Tests (NUnit) | `tests/FatturaFacile.Tests/` |

---

## 🛡️ Coding Pattern Expectations

1. **DevExpress MVVM**:
   - ViewModels inherit from `ViewModelBase` or `DocumentViewModelBase`.
   - Use `DelegateCommand` or `AsyncCommand` for actions.
   - For UI dialogs and message boxes, resolve `IMessageBoxService` or `IDialogService` via `GetService<T>()`.
   - Implement `IDocumentModule` (`ModuleId`, `Caption`, `IsActive`) for tabbed docking documents.

2. **Cross-Border Operations**:
   - Always differentiate between Italian and Swiss requirements:
     - **IT**: SDI FatturaPA 1.2.2 XML, Codice Fiscale, Partita IVA, PEC, 7-character Codice Destinatario, D.Lgs. 66/2003 rest intervals.
     - **CH**: Swiss QR-Bill (SCOR 27-digit Modulo 10 reference), UID (`CHE-xxx.xxx.xxx`), QR-IBAN (IID 30000-31999), 0.05 CHF cash rounding, ArG / OLL 1 weekly limits.

3. **Dynamic Translations**:
   - Never hardcode user-facing strings in XAML. Always use `{loc:Translate KeyName, Default='Fallback'}`.
   - Dynamic translations are loaded from SQLite `AppTranslations` into `SqliteLocalizationService`.

4. **XML Docstrings**:
   - All public classes, interfaces, properties, methods, and constructors must have complete English XML documentation.
