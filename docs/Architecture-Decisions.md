# Architecture Decisions Record (ADR) - FatturaFacile ERP Core (.NET 11)

This document records the architectural decisions, constraints, and evolution of the **FatturaFacile** enterprise desktop ERP solution.

---

## ADR-001: Strict Clean Architecture & Solution Layering

### Status
**Accepted**

### Context
FatturaFacile requires an architecture that supports long-term enterprise maintainability, testability, and clear separation of business rules from UI and data access infrastructure.

### Decision
The solution is partitioned into four strictly decoupled concentric layers:
1. **`FatturaFacile.Domain`**: Core POCO entities, records, enums, and foundational interfaces. Zero external dependencies.
2. **`FatturaFacile.Application`**: Business orchestration, CQRS commands and queries via MediatR, FluentValidation pipelines, AutoMapper profiles, and country strategies.
3. **`FatturaFacile.Infrastructure`**: EF Core SQLite data access, `TenantSaveChangesInterceptor`, repository implementations, and Windows Hello authentication.
4. **`FatturaFacile.Wpf`**: Presentation shell using Syncfusion WPF controls, FluentDark styling, and zero code-behind MVVM.

### Consequences
- Dependencies point exclusively inward.
- The Domain and Application layers can be tested completely independent of SQLite and WPF.
- Any attempt to reference Infrastructure or WPF from Domain is caught by architecture enforcement.

---

## ADR-002: ZERO Tactical DDD Mandate (Clean POCOs Over Aggregate Roots)

### Status
**Accepted (Supersedes previous tactical DDD proposals)**

### Context
Earlier architecture proposals explored tactical Domain-Driven Design (DDD) concepts including `AggregateRoot<T>`, internal domain event collections, transactional outbox patterns, and reflection-based value objects.

In practice, this introduced:
- Extreme cognitive overhead and friction for junior and mid-level developers.
- Significant serialization and mapping complexity with AutoMapper and SQLite EF Core.
- The need for background worker services to poll and dispatch outbox messages, unnecessary for an offline-first desktop application.
- Redundant layers of abstraction around simple property mutations.

### Decision
**Tactical Domain-Driven Design is strictly PROHIBITED across the entire codebase:**
1. All domain entities are clean, pragmatic POCOs with public parameterless constructors and direct property accessors (`{ get; set; }`).
2. Aggregate roots, domain event raising inside entities, and outbox tables are banned.
3. Lightweight composite values are modeled using modern C# records (e.g. `Money(decimal Amount, string Currency)`).
4. All business workflow validation, state orchestration, and calculations reside cleanly in MediatR Command Handlers and Application Strategies.

### Consequences
- Rapid onboarding for developers of all skill levels.
- Seamless, high-performance object mapping and EF Core persistence without private field hacks.
- Predictable, sequential CQRS pipeline execution.

---

## ADR-003: Multi-Tenancy Architecture & Shared Read-Only Defaults

### Status
**Accepted**

### Context
FatturaFacile operates as a single-database offline-first application that must isolate distinct accounting entities (tenants) while providing pre-seeded, standard accounting reference data (VAT codes, chart of accounts, default templates).

### Decision
Implement a hybrid row-level multi-tenancy model backed by EF Core global query filters and interceptors:
1. **`ITenantScopedEntity`**: Enforces a mandatory `Guid TenantId { get; set; }` for tenant-private accounting records (`Invoice`, `GeneralLedgerEntry`, `HrEmployee`).
   - Query filter: `e.TenantId == _currentTenantService.TenantId`.
2. **`IOptionalTenantEntity`**: Defines reference data with a nullable `Guid? TenantId { get; set; }` (`VatCode`, `ChartOfAccount`, `Product`, `Contact`).
   - Query filter: `e.TenantId == null || e.TenantId == _currentTenantService.TenantId`.
   - Records with `TenantId == null` represent global shared defaults.
3. **`TenantSaveChangesInterceptor`**:
   - Throws `InvalidOperationException` if a tenant attempts to modify or delete a global shared record (`TenantId == null`).
   - Automatically injects the active tenant ID into newly created `ITenantScopedEntity` records.
   - Automatically maintains `CreatedAt` and `UpdatedAt` audit timestamps on `IBaseEntity`.

### Consequences
- Absolute tenant data privacy without managing separate SQLite database files.
- Safe sharing of standardized fiscal reference tables.

---

## ADR-004: Country-Specific Strategy Pattern (CH vs IT)

### Status
**Accepted**

### Context
FatturaFacile supports business operations in Switzerland and Italy, which have fundamentally different legal, rounding, invoicing, and payroll regulations:
- **Switzerland (`CH`)**: CHF currency, Swiss 0.05 cash rounding, ISO 20022 QR-Bills with SCOR references, Swissdec ELM payroll deductions.
- **Italy (`IT`)**: EUR currency, 0.01 cent rounding, SDI / FatturaPA v1.2 electronic invoicing with PEC / Codice Destinatario, INPS/IRPEF/CCNL payroll.

### Decision
Encapsulate all regional divergences behind the **Strategy Pattern** through `ICountryStrategyFactory`:
- `ICountryInvoicingStrategy` (`SwissInvoicingStrategy`, `ItalianInvoicingStrategy`)
- `ICountryTaxAndRoundingStrategy` (`SwissTaxAndRoundingStrategy`, `ItalianTaxAndRoundingStrategy`)
- `ICountryPayrollStrategy` (`SwissPayrollStrategy`, `ItalianPayrollStrategy`)

Domain models remain country-neutral, storing only the necessary optional compliance sub-objects (`InvoiceQrBillData`, `InvoiceSdiMetadata`).

### Consequences
- Adding new jurisdictions or updating regional fiscal rules does not impact existing invoicing workflows.
- Eliminates spaghetti `if/else` logic from billing and payroll calculators.

---

## ADR-005: Pure MVVM with CommunityToolkit and Syncfusion WPF

### Status
**Accepted**

### Context
Desktop ERP applications require high-throughput data grids, sleek responsive styling, and maintainable view-model separation.

### Decision
1. **Zero Code-Behind:** View code-behind files (`*.xaml.cs`) contain only `InitializeComponent()`. All user actions bind directly to ViewModel commands.
2. **CommunityToolkit.Mvvm:** ViewModels inherit from `ObservableObject` and use `[ObservableProperty]` and `[RelayCommand]` source generators.
3. **Syncfusion WPF Suite:** Visual presentation utilizes `Syncfusion.SfSkinManager.VisualStyle="FluentDark"` and `SfDataGrid` for high-performance virtualized grids with sorting and filtering.
4. **Dependency Injection:** Views and ViewModels are registered and resolved through `Microsoft.Extensions.Hosting`.

### Consequences
- Fast UI rendering with dark theme aesthetics.
- 100% testable ViewModel logic with no UI thread dependencies.

---

## ADR-006: Native Biometric Security via Windows Hello

### Status
**Accepted**

### Context
Desktop accounting databases contain sensitive financial, payroll, and personal data (PII). A standard password prompt can be easily bypassed or shared.

### Decision
Enforce hardware-backed biometric authentication at application startup in `App.xaml.cs` via `WindowsHelloAuthenticationService`, utilizing WinRT `Windows.Security.Credentials.UI.UserConsentVerifier`:
- Prompts for Windows Hello fingerprint, facial recognition, or PIN.
- If verification is denied or cancelled, application execution terminates immediately (`Shutdown(1)`).
- In development or non-biometric environments, falls back gracefully to permit local debugging and CI execution.

### Consequences
- Hardware-level protection of accounting records.
- Frictionless, passwordless sign-in for authorized enterprise users.
