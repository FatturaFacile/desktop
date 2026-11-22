// <copyright file="FlushFieldServiceOutboxCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Sends the queued field operations to the server in the order the technician acted, each with its idempotency key.
/// </summary>
public sealed record FlushFieldServiceOutboxCommand : IRequest<FieldServiceFlushResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
