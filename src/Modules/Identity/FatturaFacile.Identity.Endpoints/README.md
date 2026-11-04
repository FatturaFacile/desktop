# FatturaFacile.Identity.Endpoints

HTTP endpoint definitions, route mapping, and API contracts for the **FatturaFacile** Identity module.

## Overview

`FatturaFacile.Identity.Endpoints` delivers minimal API routing and ASP.NET Core endpoints for user authentication, session refresh, and tenant selection:

- **Authentication Endpoints**: Login, token refresh, and logout routes.
- **Tenant Management**: User tenant assignment and active workspace switching.
- **ProblemDetails Formatting**: Standardized error mappings with culture-aware localized domain error responses.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Identity.Endpoints
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Identity.Endpoints
```

## Quickstart

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

// Map identity endpoints into ASP.NET Core endpoint routing pipeline
```
