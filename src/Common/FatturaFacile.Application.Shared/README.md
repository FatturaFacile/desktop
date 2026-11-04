# FatturaFacile.Application.Shared

Shared application contracts, CQRS abstractions, MediatR pipeline behaviors, error localization, and result models for **FatturaFacile**.

## Overview

`FatturaFacile.Application.Shared` standardizes application layer patterns across all ERP bounded contexts:

- **CQRS Abstractions**: `ICommand<TResult>`, `IQuery<TResult>`, and handler contracts built on MediatR.
- **Pipeline Behaviors**: Cross-cutting `ValidationBehavior<TRequest, TResponse>` (integrating FluentValidation) and `LoggingBehavior<TRequest, TResponse>`.
- **Result & Error Handling**: Unified `Result<T>` and `Result` pattern models with integrated domain error mapping.
- **Context Services**: `ITenantContext` and `ICurrentUserContext` abstractions for user identity and ambient tenancy resolution.
- **Localization Contracts**: `IErrorLocalizer` and `ErrorLocalizer` supporting 4-language domain error localization (IT, DE, FR, EN).

## Compatibility

- Target Framework: `net11.0`
- Language Version: C# 13 / preview

## Installation

Install via the .NET CLI:

```bash
dotnet add package FatturaFacile.Application.Shared
```

Or via the Package Manager Console:

```powershell
Install-Package FatturaFacile.Application.Shared
```

## Quickstart

```csharp
using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Application.Shared.Results;
using MediatR;

public record PingCommand : ICommand<Result<string>>;

public class PingCommandHandler : ICommandHandler<PingCommand, Result<string>>
{
    public Task<Result<string>> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<string>.Success("Pong"));
    }
}
```
