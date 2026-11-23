// <copyright file="GetPurchaseOrdersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetPurchaseOrdersQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetPurchaseOrdersQueryHandler(IProcurementService service) : IRequestHandler<GetPurchaseOrdersQuery, ProcurementLoad<IReadOnlyList<PurchaseOrderView>>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<PurchaseOrderView>>?> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadOrdersAsync(request.Status, cancellationToken);
    }
}
