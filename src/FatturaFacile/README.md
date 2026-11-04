# FatturaFacile

Modular enterprise meta-package and fluent composition root for the **FatturaFacile** cross-border ERP suite.

## Overview

`FatturaFacile` provides complete modular orchestration, aggregating the Invoicing, Accounting, CRM, and Identity modules into a unified, fluent configuration API. It registers MediatR handlers, validation pipelines, AutoMapper profiles, EF Core multi-database persistence, and cryptographic identity services with a single extension method.

### Key Capabilities
- **Unified ERP Bootstrap**: Configure the entire engine with `services.AddFatturaFacile(...)`.
- **Cross-Border Support**: Italy (SDI / FatturaPA) and Switzerland (ISO 20022 QR-Bill, 0.05 CHF rounding).
- **Multi-Tenant Persistence**: Flexible database provider selection (SQLite, PostgreSQL, SQL Server).
- **Security & Localization**: Argon2id password hashing, AES-256-GCM data-at-rest protection, and 4-language localized domain errors (IT, DE, FR, EN).

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Extensions;
using FatturaFacile.Infrastructure.Common.Configuration;

var services = new ServiceCollection();

services.AddFatturaFacile(options =>
{
    options.UseDatabase(DatabaseProvider.Sqlite, "Data Source=FatturaFacile.db");
    options.ConfigureLocalization(
        supportedCultures: ["it", "de", "fr", "en"],
        defaultCulture: "it"
    );
});
```
