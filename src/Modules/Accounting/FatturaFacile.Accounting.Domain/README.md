# FatturaFacile.Accounting.Domain

Fiscal and statutory accounting domain models, general ledger aggregates, and journal entry rules for **FatturaFacile**.

## Overview

`FatturaFacile.Accounting.Domain` encapsulates core financial and general ledger domain logic for cross-border operations between Italy and Switzerland:

- **General Ledger Aggregates**: `GeneralLedgerAccount`, `GeneralLedgerEntry`, and debit/credit ledger lines.
- **Double-Entry Invariants**: Rigorous validation ensuring total debits equal total credits for every posted journal entry.
- **Dual-Jurisdiction Chart of Accounts**: Supports Italian statutory GL structures and the Swiss SME Chart of Accounts (*KMU / PME Kontenrahmen*).
- **Repository Contracts**: `IGeneralLedgerRepository` domain abstraction.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview
- Pure Domain: Zero external UI or EF Core dependencies.

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Accounting.Domain
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Accounting.Domain
```

## Quickstart

```csharp
using FatturaFacile.Accounting.Domain.Aggregates;

// General ledger entities enforce balance rules and domain invariants
```
