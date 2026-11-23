// <copyright file="GetReceivableItemsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads a page of receivable items with filters and the bucket drill-down.
/// </summary>
/// <param name="Filter">The filters.</param>
public sealed record GetReceivableItemsQuery(ReceivableItemFilter Filter) : IRequest<ReceivablesLoad<ReceivableItemPage>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
