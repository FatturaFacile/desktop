# FatturaFacile.Infrastructure

Persistence, security, and hardware adapter layer for the FatturaFacile desktop ERP solution.

## 🎯 Architectural Purpose & Rules
- **Layer Index:** Layer 3 (secondary adapter layer).
- **Dependencies:** References `FatturaFacile.Application` and `FatturaFacile.Domain`.
- **Primary Technologies:** EF Core SQLite, WinRT (UserConsentVerifier), Syncfusion PDF/Excel.
- **Rules:** Exposes implementations of Application interfaces. The Presentation layer accesses persistence exclusively through Application abstractions.

---

## 📁 Project Structure

```text
FatturaFacile.Infrastructure/
├── InfrastructureServiceRegistration.cs # DI extension method AddInfrastructureServices()
├── Persistence/
│   ├── FatturaFacileDbContext.cs       # EF Core SQLite DbContext
│   ├── TenantSaveChangesInterceptor.cs # Enforces multi-tenancy rules and read-only global defaults
│   └── DatabaseSeeder.cs               # Seeds default chart of accounts, VAT codes, demo tenants
├── Repositories/
│   ├── Repository.cs                   # Generic IRepository<T> implementation
│   └── UnitOfWork.cs                   # IUnitOfWork implementation managing transactions
└── Services/
    ├── CurrentTenantService.cs         # Tracks active tenant context in-memory
    ├── SyncfusionDocumentExportService.cs # Generates PDF invoices and Excel balance sheets
    └── WindowsHelloAuthenticationService.cs # Native WinRT biometric & PIN authentication
```

---

## 🗄️ Persistence & Multi-Tenancy

### SQLite EF Core (`FatturaFacileDbContext`)
- Operates on a local SQLite database (`fattura-facile.sqlite`).
- Global query filters isolate records per tenant:
  - Scoped entities: `e.TenantId == _currentTenantService.TenantId`
  - Optional entities: `e.TenantId == null || e.TenantId == _currentTenantService.TenantId`

### `TenantSaveChangesInterceptor`
Executes on every `SaveChanges[Async]` invocation:
1. **Global Default Immutability:** Throws an `InvalidOperationException` if an entity implementing `IOptionalTenantEntity` with `TenantId == null` is modified or deleted.
2. **Auto-Tenant Assignment:** Populates `TenantId` with the active tenant ID for new `ITenantScopedEntity` instances if `TenantId == Guid.Empty`.
3. **Audit Timestamping:** Automatically sets `CreatedAt` (on insert) and `UpdatedAt` (on update) for `IBaseEntity` implementations.

---

## 🔒 Hardware Security (Windows Hello)

`WindowsHelloAuthenticationService` interacts with the native Windows Runtime (WinRT) `Windows.Security.Credentials.UI.UserConsentVerifier`:
- Verifies biometric availability via `UserConsentVerifier.CheckAvailabilityAsync()`.
- Prompts user with native Windows Hello dialog via `UserConsentVerifier.RequestVerificationAsync()`.
- Provides graceful fallback in non-biometric environments to ensure developer productivity and CI test execution.
