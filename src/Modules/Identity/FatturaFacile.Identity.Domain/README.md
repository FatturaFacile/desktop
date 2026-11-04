# FatturaFacile.Identity.Domain

Identity, user, tenant, refresh token, and security audit log domain models for **FatturaFacile**.

## Overview

`FatturaFacile.Identity.Domain` contains core security and tenant domain entities compliant with Swiss nLPD / FADP and EU GDPR standards:

- **Aggregates**: `User`, `Tenant`, `RefreshToken`, and `SecurityAuditLog`.
- **Brute-Force & Lockout Rules**: Configurable threshold tracking with `LockoutEndUtc`.
- **Repository Contracts**: `IUserRepository`, `ITenantRepository`, `IRefreshTokenRepository`, and `ISecurityAuditLogRepository`.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview
- Pure Domain: Zero UI or EF Core dependencies.

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Identity.Domain
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Identity.Domain
```

## Quickstart

```csharp
using FatturaFacile.Identity.Domain.Aggregates;

// Identity entities encapsulate credential and lockout rules
```
