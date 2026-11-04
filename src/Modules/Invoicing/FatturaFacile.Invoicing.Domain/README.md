# FatturaFacile.Invoicing.Domain

Cross-border sales and invoicing domain models, aggregates, calculation services, and tax rules for **FatturaFacile**.

## Overview

`FatturaFacile.Invoicing.Domain` models the core invoicing domain with full support for Italian and Swiss fiscal compliance:

- **Aggregates & Entities**: `Invoice` Aggregate Root, `InvoiceItem` line items, and invoice status state machine.
- **Cross-Border Billing Rules**:
  - **Italy**: SDI FatturaPA 1.2.2 compliance, split payment, and withholding tax logic.
  - **Switzerland**: ISO 20022 QR-Bill SCOR Modulo 10 reference generation, QR-IBAN validation, and Swiss cash rounding (0.05 CHF).
- **Calculation Services**: Deterministic calculation of Net, Tax, Gross, and Swiss Rounding Deltas.
- **Repository Contracts**: `IInvoiceRepository`.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview
- Pure Domain: Zero UI or EF Core dependencies.

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Invoicing.Domain
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Invoicing.Domain
```

## Quickstart

```csharp
using FatturaFacile.Invoicing.Domain.Aggregates;

// Invoice aggregate encapsulates line item calculations and status transitions
```
