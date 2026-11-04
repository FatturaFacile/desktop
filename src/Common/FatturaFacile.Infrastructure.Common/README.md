# FatturaFacile.Infrastructure.Common

Shared infrastructure services, EF Core interceptors, transactional outbox storage, multi-database provider support, and multi-tenant persistence infrastructure for **FatturaFacile**.

## Overview

`FatturaFacile.Infrastructure.Common` provides the shared persistence and telemetry backbone for all module-specific EF Core DbContexts:

- **Entity Framework Interceptors**:
  - `TenantSaveChangesInterceptor`: Enforces tenant boundary isolation and auto-populates `TenantId`.
  - `AuditAndSoftDeleteInterceptor`: Automatically tracks `CreatedAt`, `UpdatedAt`, `DeletedAt`, and manages soft-delete flags.
  - `OutboxDomainEventsInterceptor`: Captures domain events from Aggregate Roots and serializes them into outbox messages in the same transaction.
- **Multi-Database Support**: Helpers for SQLite, PostgreSQL (`Npgsql`), and Microsoft SQL Server database configurations.
- **Transactional Outbox**: Entity model and schema mapping for `OutboxMessage`.
- **Diagnostics & Observability**: Integrated OpenTelemetry `ActivitySource` and `Meter` metrics tracking ERP transactions.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Infrastructure.Common
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Infrastructure.Common
```

## Quickstart

```csharp
using Microsoft.EntityFrameworkCore;
using FatturaFacile.Infrastructure.Common.Interceptors;

public static void ConfigureDbContext(DbContextOptionsBuilder options, IServiceProvider sp)
{
    options.AddInterceptors(
        sp.GetRequiredService<TenantSaveChangesInterceptor>(),
        sp.GetRequiredService<AuditAndSoftDeleteInterceptor>(),
        sp.GetRequiredService<OutboxDomainEventsInterceptor>()
    );
}
```
