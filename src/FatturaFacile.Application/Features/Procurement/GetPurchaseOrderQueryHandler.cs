// <copyright file="GetPurchaseOrderQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetPurchaseOrderQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetPurchaseOrderQueryHandler(IProcurementService service) : IRequestHandler<GetPurchaseOrderQuery, ProcurementLoad<PurchaseOrderView>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<PurchaseOrderView>?> Handle(GetPurchaseOrderQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadOrderAsync(request.OrderId, cancellationToken);
    }
}
