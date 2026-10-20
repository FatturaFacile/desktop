# FatturaFacile - System Architecture & Technical Specifications

This document outlines the architectural patterns, runtime mechanisms, and design decisions governing **FatturaFacile**.

---

## 1. Clean Architecture Principles

The solution enforces a strict inward dependency rule:

```
[ FatturaFacile (WPF) ] ────┐
                            ├──> [ FatturaFacile.Application ] ──> [ FatturaFacile.Domain ]
[ FatturaFacile.Infrastructure ] ┘
```

### Layer Responsibilities

1. **`FatturaFacile.Domain` (Core Domain)**
   - Contains all domain logic, rich entities with private setters, business invariants, domain events, value objects, and repository interfaces.
   - **Zero external dependencies**: Depends only on the .NET runtime (`System.*`). No references to EF Core, ASP.NET Core, or WPF libraries.

2. **`FatturaFacile.Application` (Business Orchestration)**
   - Organizes application use cases using the **CQRS (Command Query Responsibility Segregation)** pattern with **MediatR**.
   - Contains DTOs, concrete AutoMapper profiles, FluentValidation validators, and pipeline behaviors.
   - Depends solely on `FatturaFacile.Domain`.

3. **`FatturaFacile.Infrastructure` (Data Access & Exporters)**
   - Implements interfaces defined in the Domain and Application layers.
   - Contains EF Core SQLite configuration (`FatturaFacileDbContext`), repository implementations, PBKDF2 password hasher, dynamic SQLite localization store, and export engines (FatturaPA XML, Swiss QR-Bill SVG, ISO 20022 `pain.001`, `camt.053`).
   - Depends on `FatturaFacile.Application` and `FatturaFacile.Domain`.

4. **`FatturaFacile` (Presentation Layer)**
   - Pure WPF MVVM client utilizing Microsoft's `CommunityToolkit.Mvvm`.
   - Composed of Views, ViewModels, dynamic theme dictionaries (`DarkTheme.xaml` and `LightTheme.xaml`), custom markup extensions (`TranslateExtension`), and dependency injection composition root (`App.xaml.cs`).
   - References `FatturaFacile.Application` and `FatturaFacile.Infrastructure`.

---

## 2. CQRS & MediatR Pipeline Architecture

Every user action or background query flows through MediatR pipeline behaviors registered in strict sequence:

```mermaid
sequenceDiagram
    participant UI as WPF View / ViewModel
    participant Med as MediatR
    participant Log as LoggingBehavior
    participant Ten as TenantBehavior
    participant Val as ValidationBehavior
    participant Tx as TransactionBehavior
    participant Hdl as Command / Query Handler
    participant DB as SQLite DbContext

    UI->>Med: Send(Command or Query)
    Med->>Log: Log Execution
    Log->>Ten: Verify Tenant Context
    Ten->>Val: Run FluentValidation Rules
    Val->>Tx: Begin Database Transaction
    Tx->>Hdl: Execute Handler
    Hdl->>DB: Persist Changes via Repositories
    DB-->>Hdl: Saved State
    Hdl-->>Tx: Return Result
    Tx-->>Med: Commit Transaction
    Med-->>UI: Return DTO / Success Result
```

### Pipeline Behaviors:
- **`LoggingBehavior<TRequest, TResponse>`**: Measures execution time, logs structured warnings for slow requests (>500ms).
- **`TenantBehavior<TRequest, TResponse>`**: Validates that a tenant is currently selected before processing tenant-scoped commands.
- **`ValidationBehavior<TRequest, TResponse>`**: Executes all FluentValidation validators registered for the request; fails fast by throwing a `ValidationException` containing structured errors.
- **`TransactionBehavior<TRequest, TResponse>`**: Wraps modifying commands inside a database transaction, guaranteeing atomic rollbacks on failure.

---

## 3. Multi-Tenancy & Data Isolation

FatturaFacile supports complete multi-tenant isolation within a shared SQLite database:

1. **`ITenantEntity` Interface**:
   ```csharp
   public interface ITenantEntity
   {
       Guid TenantId { get; set; }
   }
   ```
2. **EF Core Global Query Filters**:
   In `FatturaFacileDbContext.OnModelCreating()`, all entities implementing `ITenantEntity` automatically receive a query filter:
   ```csharp
   modelBuilder.Entity<Client>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
   ```
3. **Dynamic Tenant Switching**:
   When the user switches tenants using the header dropdown, `ITenantProvider.SetCurrentTenant(newTenantId)` updates the current context and signals the UI to refresh without requiring an application restart.

---

## 4. Value Objects & Cross-Border Invariants

Cross-border operations between Italy and Switzerland require strict domain validation:

| Value Object | Validations & Business Rules |
| :--- | :--- |
| **`Money`** | Immutable `(decimal Amount, Currency Currency)`. Multi-currency operations between EUR and CHF are blocked. Features statutory Swiss cash rounding to the nearest 0.05 (`ToSwissCashRounding()`). |
| **`FiscalCode`** | Validates Italian 16-character alphanumeric Codice Fiscale or 11-digit temporary/numeric codes via regex. |
| **`VatNumber`** | Validates Italian 11-digit Partita IVA (`ITxxxxxxxxxxx`) and Swiss Business Identification Numbers (`CHE-xxx.xxx.xxx MWST/TVA/IVA`). |
| **`QrIban`** | Validates Swiss/Liechtenstein QR-IBANs (21 characters starting with `CH` or `LI`) with Institute Identification (IID) falling strictly in the range **30000 to 31999**. |
| **`Address`** | Encapsulates street, postal code, city, province/canton, and ISO country code (`IT`, `CH`). |

---

## 5. Dynamic SQLite Localization (i18n)

Unlike traditional static `.resx` files, FatturaFacile stores translations in SQLite, allowing users and administrators to customize translations at runtime:

1. **Storage Schema**:
   `AppTranslations (Id, TenantId, Locale, Key, DefaultValue, CustomValue, UpdatedAt)`
2. **In-Memory Cache**:
   `SqliteLocalizationService` holds an in-memory `ConcurrentDictionary<(string Locale, string Key), string>` loaded on startup for zero-overhead string resolution.
3. **XAML Markup Extension**:
   `{loc:Translate KeyName, Default='Default Text'}` subscribes to `LocalizationManager.LanguageChanged` events to update UI bindings instantly.
4. **Admin UI**:
   Administrators can search, edit, and persist translations via the Localization Management view.

---

## 6. Security & Session Management

- **Password Storage**: Uses `PBKDF2` with HMAC-SHA512, 100,000 iterations, and a cryptographically secure 16-byte salt.
- **Session Timeout**: The application monitors user activity and triggers a secure session lock upon 15 minutes of inactivity, requiring re-authentication.
- **RBAC (Role-Based Access Control)**: Restricts access to sensitive views (such as general ledger entries, payroll records, and system administration) based on `UserRole`.
