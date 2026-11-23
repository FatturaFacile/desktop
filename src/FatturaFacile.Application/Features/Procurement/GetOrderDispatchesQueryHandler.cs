// <copyright file="GetOrderDispatchesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetOrderDispatchesQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetOrderDispatchesQueryHandler(IProcurementService service) : IRequestHandler<GetOrderDispatchesQuery, ProcurementLoad<IReadOnlyList<DispatchView>>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<DispatchView>>?> Handle(GetOrderDispatchesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadDispatchesAsync(request.OrderId, cancellationToken);
    }
}
