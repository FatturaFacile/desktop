# Architecture Decisions Record (ADR) - FatturaFacile ERP Core (.NET 11)

## Context & Objectives
FatturaFacile has been engineered as a high-throughput, enterprise-grade modular ERP core built on **.NET 11 (C# 13 preview)** and **Clean Architecture / Domain-Driven Design (DDD)** principles. The library is distributed as modular NuGet packages (`FatturaFacile.Domain.Shared`, `FatturaFacile.Application.Shared`, `FatturaFacile.Infrastructure.Common`, `FatturaFacile.Invoicing.*`, `FatturaFacile.Accounting.*`, `FatturaFacile.Crm.*`, `FatturaFacile.Identity.*`) as well as an orchestrating meta-package (`FatturaFacile`) and a WPF client (`FatturaFacile.Desktop.Wpf`).

---

## 1. Multi-Database Persistence Strategy (EF Core)

### Supported Database Engines:
- **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`)
- **PostgreSQL** (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Microsoft SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer`)

### Provider Divergence & Compatibility Decisions:
1. **Concurrency Tokens (`RowVersion`):**
   - SQLite does not have native byte-array timestamp row versions. Default timestamps and optimistic concurrency tokens are mapped uniformly across providers.
2. **Date & Time Standards:**
   - All global audit timestamps and domain events utilize `DateTimeOffset` in UTC (`DateTimeOffset.UtcNow`).
   - Business document dates (invoice date, due date) are modeled as .NET `DateOnly` to eliminate timezone drift issues across international jurisdictions (Italy & Switzerland).
3. **Decimals & Monetary Precision:**
   - Prices, Net, Tax, and Gross totals are typed as high-precision `decimal(18, 2)` or `decimal(18, 4)` for unit prices, and quantities as `decimal(14, 4)` to eliminate floating-point inaccuracies.
4. **Swiss 5-Centimes Rounding Rule:**
   - In accordance with Swiss billing rules (QR-Bill and CHF currency), cash amounts are systematically rounded to the nearest 0.05 CHF using `decimal.Round(unrounded * 20m) / 20m`, and the rounding delta is explicitly tracked in `RoundingAmount`.

---

## 2. Modular Monolith & Clean Architecture

Each Bounded Context is organized into three strictly bounded sub-assemblies:
1. **Domain:** Pure business models, Aggregates, Value Objects, Domain Events, and specialized Repository contracts (`IInvoiceRepository`, `IContactRepository`, `IGeneralLedgerRepository`). Zero dependencies on EF Core or UI.
2. **Application:** CQRS Commands and Queries mediated via **MediatR**, cross-cutting validation pipelines via **FluentValidation**, and DTO projections via **AutoMapper**.
3. **Infrastructure:** EF Core `DbContext`, `IEntityTypeConfiguration<T>` mappings, repository implementations, and database connection factories.

Cross-boundary integrity is permanently verified via automated architectural unit tests powered by **NetArchTest.Rules**.

---

## 3. Reliability & Operational Governance

- **Transactional Outbox Pattern:**
  - When domain events occur on Aggregate Roots, an EF Core `SaveChangesInterceptor` (`OutboxDomainEventsInterceptor`) serializes them into the `outbox_messages` table within the same transaction, guaranteeing at-least-once delivery for inter-module integration events without two-phase commits.
- **Audit Logging Interceptor:**
  - Any entity implementing `IAuditableEntity` or `ISoftDeletable` automatically captures audit timestamps (`CreatedAt`, `UpdatedAt`) and soft-deletion flags (`DeletedAt`, `IsDeleted`) transparently.
- **Observability:**
  - Pre-instrumented with `System.Diagnostics.ActivitySource` and `System.Diagnostics.Metrics.Meter` for enterprise OpenTelemetry tracing and Prometheus/OTLP metric scraping.

---

## 4. Fluent Consumption Interface

Consumers in ASP.NET Core or background worker hosts configure the entire ERP engine with fluent syntax:

```csharp
services.AddFatturaFacile(options =>
{
    options.UseDatabase(DatabaseProvider.Sqlite, connectionString);
    options.ConfigureLocalization(
        supportedCultures: ["it", "de", "fr", "en"],
        defaultCulture: "it"
    );
});
```

---

## 5. 4-Language Localization Architecture (EN, IT, DE, FR)

### Decoupled Domain & Application Errors
- Domain and Application layers remain purely decoupled from ASP.NET Core and HTTP dependencies.
- Business domain failures return strongly typed `DomainErrorDefinition` records (e.g., `DomainErrors.Invoice.NotFound`, `DomainErrors.Identity.InvalidCredentials`).
- `IErrorLocalizer` maps semantic error codes to culture-specific strings based on `ICurrentUserContext.Culture` (falling back to `"en"`).
- Supported languages:
  - **English (EN)**: Default fallback.
  - **Italian (IT)**: Complete Italian domain business and validation messages.
  - **German (DE)**: Complete German domain business and validation messages.
  - **French (FR)**: Complete French domain business and validation messages.

---

## 6. Multi-Tenancy Architecture

### Strict Isolation & Compile-time Enforcement
- `IMustHaveTenant`: Enforces a mandatory `TenantId` on all tenant-scoped entities.
- `IMayHaveTenant`: Supports nullable tenant associations for host-level or administrative entities.
- `ITenantContext`: Scoped service resolving the active tenant from HTTP headers, claims, or client context.
- `TenantSaveChangesInterceptor`:
  - Enforces tenant integrity on EF Core `SaveChanges`.
  - Blocks cross-tenant mutations.
  - Injects `TenantId` automatically if not explicitly set.
- Global Query Filters:
  - Applied automatically on all tenant entities:
    ```csharp
    builder.Entity<T>().HasQueryFilter(e => _tenantContext.IsTenantFilterIgnored || e.TenantId == _tenantContext.TenantId);
    ```

---

## 7. Identity & Swiss nLPD / FADP Compliance (`FatturaFacile.Identity`)

### Cryptographic Standards & Personal Data Protection
1. **Argon2id Password Hashing (`IPasswordHasher`):**
   - High-memory Argon2id variant (`m=65536` [64 MB], `t=4` iterations, `p=2` parallelism threads).
   - Constant-time verification to eliminate side-channel timing attacks.
2. **Data-at-Rest Encryption for PII:**
   - Personally Identifiable Information (such as email addresses) is encrypted using **AES-256-GCM** authenticated symmetric cipher.
   - Fast $O(1)$ search lookups are enabled via SHA-256 blind indexing (`NormalizedEmail`), complying with Swiss nLPD / FADP data minimization rules.
3. **Account Protection & Rate-Limiting Lockout:**
   - Brute-force protection automatically triggers an account lockout upon reaching 5 failed login attempts (`LockoutEndUtc`).
   - Audit trail records login attempts, lockouts, and credential rotations into `security_audit_logs`.
4. **Token Lifecycle & Security:**
   - JWT Access Tokens carry standard claims (`sub`, `tenant_id`, `email`, `locale`).
   - Refresh tokens are hashed and cryptographically rotated upon renewal. Reused or revoked tokens immediately invalidate the session chain.
