// <copyright file="GetMaintenanceContractsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetMaintenanceContractsQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetMaintenanceContractsQueryHandler(IFieldServiceStore store) : IRequestHandler<GetMaintenanceContractsQuery, FieldServiceCached<IReadOnlyList<MaintenanceContract>>?>
{
    /// <inheritdoc/>
    public Task<FieldServiceCached<IReadOnlyList<MaintenanceContract>>?> Handle(GetMaintenanceContractsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FieldServiceCacheReader.ReadAsync(store, FieldServiceCacheKinds.Contracts, FieldServiceCacheKinds.All, FieldServiceContractParser.ParseContracts, cancellationToken);
    }
}
