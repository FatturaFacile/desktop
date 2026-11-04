# FatturaFacile.Accounting.Infrastructure

Entity Framework Core persistence layer, database mappings, and repository implementations for the **FatturaFacile** Accounting module.

## Overview

`FatturaFacile.Accounting.Infrastructure` implements the persistence contracts defined in the Accounting domain and application layers:

- **Entity Mappings**: Explicit EF Core Fluent API configurations for `GeneralLedgerAccount`, `GeneralLedgerEntry`, and ledger lines.
- **Repositories**: `GeneralLedgerRepository` implementation adhering to multi-tenant isolation and transactional safety.
- **DbContext Integration**: Multi-database provider compatibility (SQLite, PostgreSQL, SQL Server).

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Accounting.Infrastructure
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Accounting.Infrastructure
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Accounting.Domain.Repositories;
using FatturaFacile.Accounting.Infrastructure.Repositories;

services.AddScoped<IGeneralLedgerRepository, GeneralLedgerRepository>();
```
