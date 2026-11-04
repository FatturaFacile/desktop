# FatturaFacile.Crm.Domain

Customer and supplier relationship domain models, dual-jurisdiction address representations, and contact aggregates for **FatturaFacile**.

## Overview

`FatturaFacile.Crm.Domain` contains the business entities and aggregates representing commercial partners across Italy and Switzerland:

- **Contact & Company Aggregates**: Dual-mode entities supporting B2B and B2C clients and suppliers.
- **Cross-Border Tax Identifiers**: Italian Partita IVA, Codice Fiscale, SDI destination codes, PEC emails, and Swiss enterprise UID (`CHE-xxx.xxx.xxx`).
- **Repository Abstractions**: `IContactRepository` contract for querying and storing business partners.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview
- Pure Domain: Zero UI or EF Core dependencies.

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Crm.Domain
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Crm.Domain
```

## Quickstart

```csharp
using FatturaFacile.Crm.Domain.Aggregates;

// Encapsulated contact entities with validation invariants
```
