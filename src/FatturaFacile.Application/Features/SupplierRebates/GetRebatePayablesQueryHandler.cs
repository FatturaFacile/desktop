// <copyright file="GetRebatePayablesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="GetRebatePayablesQuery"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class GetRebatePayablesQueryHandler(ISupplierRebatesSyncService sync) : IRequestHandler<GetRebatePayablesQuery, RebatePayablesResult>
{
    /// <inheritdoc/>
    public Task<RebatePayablesResult> Handle(GetRebatePayablesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return sync.LoadPayablesAsync(request.SupplierId, cancellationToken);
    }
}
