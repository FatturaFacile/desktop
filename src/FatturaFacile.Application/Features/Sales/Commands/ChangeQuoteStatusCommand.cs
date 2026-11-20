// <copyright file="ChangeQuoteStatusCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Sales;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Command that moves a quote through its workflow (submit, approve, send, accept, reject, expire).
/// </summary>
/// <param name="QuoteId">The quote identifier.</param>
/// <param name="TargetStatus">The target status, one of <see cref="QuoteStatuses"/>.</param>
public sealed record ChangeQuoteStatusCommand(Guid QuoteId, string TargetStatus) : IRequest<QuoteDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission =>
        string.Equals(TargetStatus, QuoteStatuses.Approved, StringComparison.Ordinal) ? Permission.InvoicesIssue : Permission.InvoicesWrite;
}
