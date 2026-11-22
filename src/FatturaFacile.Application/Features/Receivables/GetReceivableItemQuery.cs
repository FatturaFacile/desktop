// <copyright file="GetReceivableItemQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads one receivable item with its allocations.
/// </summary>
/// <param name="ItemId">The item.</param>
public sealed record GetReceivableItemQuery(Guid ItemId) : IRequest<ReceivablesLoad<ReceivableItem>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
