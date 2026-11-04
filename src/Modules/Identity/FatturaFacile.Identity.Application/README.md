# FatturaFacile.Identity.Application

Authentication and authorization use cases, Argon2id password hashing, AES-256-GCM data protection, and JWT token issuance for **FatturaFacile**.

## Overview

`FatturaFacile.Identity.Application` implements enterprise identity and token lifecycle services:

- **Commands & Queries**: User authentication, registration, tenant switching, and token refreshing.
- **Cryptographic Services**:
  - `Argon2idPasswordHasher`: Memory-hard Argon2id key derivation (`m=65536`, `t=4`, `p=2`).
  - `DataProtectorService`: AES-256-GCM encryption with SHA-256 blind indexing for sensitive personal data (Swiss nLPD / FADP compliance).
  - `TokenService`: Cryptographically signed JWT access tokens and hashed refresh token rotation.

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Identity.Application
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Identity.Application
```

## Quickstart

```csharp
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Identity.Application.Security;

services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
services.AddSingleton<IDataProtectorService, DataProtectorService>();
services.AddSingleton<ITokenService, TokenService>();
```
