// <copyright file="SetReceivableDisputeCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Marks or clears the dispute of an item. Saved in the outbox first; setting the same state twice is harmless.
/// </summary>
/// <param name="Request">The dispute.</param>
public sealed record SetReceivableDisputeCommand(DisputeRequest Request) : IRequest<ReceivablesActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
