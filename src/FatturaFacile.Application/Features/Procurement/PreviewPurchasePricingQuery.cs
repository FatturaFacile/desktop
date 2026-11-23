// <copyright file="PreviewPurchasePricingQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Previews the pricing of a purchase line, from the server or, offline, computed locally.
/// </summary>
/// <param name="Request">The inputs.</param>
public sealed record PreviewPurchasePricingQuery(
    PricingPreviewRequest Request) : IRequest<ProcurementLoad<PricingPreview>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
