# FatturaFacile.Domain.Shared

Core domain primitives, value objects, domain error abstractions, and multi-tenant domain contracts for the **FatturaFacile** cross-border ERP ecosystem.

## Overview

`FatturaFacile.Domain.Shared` provides the zero-dependency domain foundations consumed by all business bounded contexts (Invoicing, Accounting, CRM, Identity) within FatturaFacile. It defines:

- **Entity Primitives & Aggregates**: Base `Entity<TId>` and `AggregateRoot<TId>` with integrated domain event dispatching.
- **Value Objects**: Pure immutable value objects including multi-currency `Money` supporting Swiss 5-centimes (`0.05 CHF`) rounding rules.
- **Tenancy Contracts**: `ITenantEntity`, `IMustHaveTenant`, and `IMayHaveTenant` interfaces.
- **Auditing & Lifecycle**: `IAuditableEntity` and `ISoftDeletable` abstractions.
- **Domain Errors & Events**: Strongly typed `DomainErrorDefinition`, `DomainErrors`, and `IDomainEvent` primitives.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview
- Dependencies: None (Pure .NET BCL)

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Domain.Shared
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Domain.Shared
```

## Quickstart

```csharp
using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Domain.Shared.ValueObjects;
using FatturaFacile.Domain.Shared.Abstractions;

public sealed class SampleOrder : AggregateRoot<Guid>, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public Money Total { get; private set; }

    public SampleOrder(Guid id, Guid tenantId, Money total) : base(id)
    {
        TenantId = tenantId;
        Total = total;
    }
}
```
