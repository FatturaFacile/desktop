// <copyright file="GetProjectBillingSnapshotQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectBilling;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectBilling;

/// <summary>
/// Handles <see cref="GetProjectBillingSnapshotQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetProjectBillingSnapshotQueryHandler(IProjectBillingSnapshotStore store) : IRequestHandler<GetProjectBillingSnapshotQuery, ProjectBillingSnapshot?>
{
    /// <inheritdoc/>
    public Task<ProjectBillingSnapshot?> Handle(GetProjectBillingSnapshotQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(request.ProjectId, cancellationToken);
    }

    private async Task<ProjectBillingSnapshot?> LoadAsync(string projectId, CancellationToken cancellationToken)
    {
        var stored = await store.FindAsync(projectId, cancellationToken).ConfigureAwait(false);

        return stored is null ? null : ProjectBillingParser.ParseSnapshot(projectId, stored.Payload, stored.SyncedAtUtc);
    }
}
