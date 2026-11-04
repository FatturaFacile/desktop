# FatturaFacile.Invoicing.Infrastructure

Entity Framework Core persistence layer, database configuration, and repository implementations for the **FatturaFacile** Invoicing module.

## Overview

`FatturaFacile.Invoicing.Infrastructure` provides high-performance data access and repository implementations for invoices and line items:

- **Entity Mappings**: Explicit EF Core Fluent API mappings for `Invoice` and `InvoiceItem`.
- **Repositories**: `InvoiceRepository` with multi-tenant filtering and sequence generation.
- **DbContext Integration**: Multi-database provider compatibility (SQLite, PostgreSQL, SQL Server).

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Invoicing.Infrastructure
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Invoicing.Infrastructure
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Invoicing.Domain.Repositories;
using FatturaFacile.Invoicing.Infrastructure.Repositories;

services.AddScoped<IInvoiceRepository, InvoiceRepository>();
```
