# FatturaFacile.Crm.Application

Application orchestration, CQRS commands, queries, and CRM workflow handlers for **FatturaFacile**.

## Overview

`FatturaFacile.Crm.Application` coordinates customer and supplier lifecycle operations:

- **CQRS Handlers**: Registration, search, update, and lookup operations for commercial contacts.
- **Cross-Border Validation**: FluentValidation rules verifying fiscal codes, VAT numbers, Swiss UIDs, and PEC formats.
- **DTOs & Projections**: Standardized DTOs for CRM integration across desktop and web endpoints.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Crm.Application
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Crm.Application
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;

// Handlers are automatically discovered when registering CRM application services
```
