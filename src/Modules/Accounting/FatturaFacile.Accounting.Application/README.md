# FatturaFacile.Accounting.Application

Application orchestration, CQRS commands, queries, and accounting workflow handlers for **FatturaFacile**.

## Overview

`FatturaFacile.Accounting.Application` coordinates general ledger operations, journal entry posting workflows, and accounting reports across tenants:

- **CQRS Handlers**: Commands and queries for creating accounts, posting journal entries, and generating fiscal balances.
- **Validation Rules**: FluentValidation validators enforcing double-entry balance and account integrity.
- **DTOs & Projections**: Clean application contracts decoupled from persistent data models.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Accounting.Application
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Accounting.Application
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;

// Handlers are automatically registered when configuring MediatR with this assembly
```
