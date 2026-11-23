// <copyright file="GetOrderConfirmationsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetOrderConfirmationsQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetOrderConfirmationsQueryHandler(IProcurementService service) : IRequestHandler<GetOrderConfirmationsQuery, ProcurementLoad<IReadOnlyList<SupplierConfirmationView>>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<SupplierConfirmationView>>?> Handle(GetOrderConfirmationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadConfirmationsAsync(request.OrderId, cancellationToken);
    }
}
