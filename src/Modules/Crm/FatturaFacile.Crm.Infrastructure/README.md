# FatturaFacile.Crm.Infrastructure

Entity Framework Core persistence layer, database configuration, and repository implementations for the **FatturaFacile** CRM module.

## Overview

`FatturaFacile.Crm.Infrastructure` delivers the concrete database persistence and query pipeline for customer and supplier data:

- **Entity Mappings**: Explicit EF Core Fluent API mappings for contacts, addresses, and tax identifiers.
- **Repositories**: `ContactRepository` implementation guaranteeing multi-tenant scoping and indexing.
- **DbContext Integration**: Multi-database provider compatibility (SQLite, PostgreSQL, SQL Server).

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Crm.Infrastructure
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Crm.Infrastructure
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Crm.Domain.Repositories;
using FatturaFacile.Crm.Infrastructure.Repositories;

services.AddScoped<IContactRepository, ContactRepository>();
```
