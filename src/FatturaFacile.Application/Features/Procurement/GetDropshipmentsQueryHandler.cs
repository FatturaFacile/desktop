// <copyright file="GetDropshipmentsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetDropshipmentsQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetDropshipmentsQueryHandler(IProcurementService service) : IRequestHandler<GetDropshipmentsQuery, ProcurementLoad<IReadOnlyList<DropshipmentView>>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<DropshipmentView>>?> Handle(GetDropshipmentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadDropshipmentsAsync(request.Status, cancellationToken);
    }
}
