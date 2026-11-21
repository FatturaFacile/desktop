// <copyright file="GetDssSnapshotQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;
using MediatR;

namespace FatturaFacile.Application.Features.Dss;

/// <summary>
/// Handler for <see cref="GetDssSnapshotQuery"/>.
/// </summary>
/// <param name="store">The local snapshot cache.</param>
public sealed class GetDssSnapshotQueryHandler(IDssSnapshotStore store) : IRequestHandler<GetDssSnapshotQuery, DssSnapshot?>
{
    /// <inheritdoc/>
    public Task<DssSnapshot?> Handle(GetDssSnapshotQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(request.Filter, cancellationToken);
    }

    private async Task<DssSnapshot?> LoadAsync(DssFilter filter, CancellationToken cancellationToken)
    {
        var stored = await store.FindAsync(filter, cancellationToken).ConfigureAwait(false);
        if (stored is null)
        {
            return null;
        }

        var payload = stored.Payload;
        var overview = DssSnapshotParser.ParseOverview(payload.OverviewJson);

        return new DssSnapshot(
            filter,
            overview,
            DssSnapshotParser.ParseProfitability(payload.ProfitabilityJson),
            DssSnapshotParser.ParseVarianceBridge(payload.VarianceBridgeJson),
            DssSnapshotParser.ParseBaseline(overview, payload.SimulationInputsJson),
            DssSnapshotParser.ParseLegalEntities(payload.LegalEntitiesJson),
            stored.SyncedAtUtc);
    }
}
