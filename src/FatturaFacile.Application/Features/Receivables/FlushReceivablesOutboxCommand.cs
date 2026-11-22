// <copyright file="FlushReceivablesOutboxCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Sends the queued receivables actions, oldest first, once the server is reachable again.
/// </summary>
public sealed record FlushReceivablesOutboxCommand : IRequest<ReceivablesFlushResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
