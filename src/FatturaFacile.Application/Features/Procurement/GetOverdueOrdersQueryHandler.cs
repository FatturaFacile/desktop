// <copyright file="GetOverdueOrdersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetOverdueOrdersQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetOverdueOrdersQueryHandler(IProcurementService service) : IRequestHandler<GetOverdueOrdersQuery, ProcurementLoad<IReadOnlyList<OverdueOrderView>>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<OverdueOrderView>>?> Handle(GetOverdueOrdersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadOverdueAsync(cancellationToken);
    }
}
