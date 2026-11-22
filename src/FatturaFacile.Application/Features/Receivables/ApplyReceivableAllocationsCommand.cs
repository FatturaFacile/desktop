// <copyright file="ApplyReceivableAllocationsCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Applies payments, credit notes, write-offs, discounts or debit adjustments to open items. Saved in the outbox first and sent when the server is reachable; replays are safe.
/// </summary>
/// <param name="Request">The allocations.</param>
public sealed record ApplyReceivableAllocationsCommand(ApplyAllocationsRequest Request) : IRequest<ReceivablesActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
