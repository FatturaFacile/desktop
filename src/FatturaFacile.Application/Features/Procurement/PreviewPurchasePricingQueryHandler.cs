// <copyright file="PreviewPurchasePricingQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="PreviewPurchasePricingQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class PreviewPurchasePricingQueryHandler(IProcurementService service) : IRequestHandler<PreviewPurchasePricingQuery, ProcurementLoad<PricingPreview>>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<PricingPreview>> Handle(PreviewPurchasePricingQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.PreviewPricingAsync(request.Request, cancellationToken);
    }
}
