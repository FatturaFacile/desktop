# FatturaFacile.Identity.Infrastructure

Entity Framework Core persistence layer, database mappings, and repository implementations for the **FatturaFacile** Identity module.

## Overview

`FatturaFacile.Identity.Infrastructure` provides the database layer for tenant and user management:

- **Entity Mappings**: Explicit EF Core Fluent API configurations for tenants, users, refresh tokens, and security audit logs.
- **Repositories**: `TenantRepository`, `UserRepository`, `RefreshTokenRepository`, and `SecurityAuditLogRepository`.
- **DbContext Integration**: Multi-database provider compatibility (SQLite, PostgreSQL, SQL Server).

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Identity.Infrastructure
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Identity.Infrastructure
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Repositories;

services.AddScoped<ITenantRepository, TenantRepository>();
services.AddScoped<IUserRepository, UserRepository>();
```
