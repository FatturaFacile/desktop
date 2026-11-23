// <copyright file="GetDropshipmentQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetDropshipmentQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetDropshipmentQueryHandler(IProcurementService service) : IRequestHandler<GetDropshipmentQuery, ProcurementLoad<DropshipmentView>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<DropshipmentView>?> Handle(GetDropshipmentQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadDropshipmentAsync(request.DropshipmentId, cancellationToken);
    }
}
