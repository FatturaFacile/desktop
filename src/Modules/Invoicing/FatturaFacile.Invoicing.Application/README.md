# FatturaFacile.Invoicing.Application

CQRS orchestration, MediatR commands, queries, AutoMapper profiles, and validation workflows for the **FatturaFacile** Invoicing module.

## Overview

`FatturaFacile.Invoicing.Application` coordinates the invoicing lifecycle from draft creation to issuance and settlement:

- **Commands & Queries**:
  - `CreateInvoiceCommand`, `GetInvoiceByIdQuery`, `GetInvoicesQuery`.
  - Status management (issue, pay, cancel).
- **Validation Rules**: FluentValidation checking line totals, customer jurisdiction tax fields, and currency rules.
- **AutoMapper Profiles**: `InvoicingMappingProfile` converting aggregates into projection DTOs.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Invoicing.Application
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Invoicing.Application
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Invoicing.Application.Mappings;

services.AddAutoMapper(typeof(InvoicingMappingProfile).Assembly);
```
