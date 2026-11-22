// <copyright file="GetProjectEvmSnapshotQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectEvm;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectEvm;

/// <summary>
/// Handles <see cref="GetProjectEvmSnapshotQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetProjectEvmSnapshotQueryHandler(IProjectEvmSnapshotStore store) : IRequestHandler<GetProjectEvmSnapshotQuery, ProjectEvmSnapshot?>
{
    /// <inheritdoc/>
    public Task<ProjectEvmSnapshot?> Handle(GetProjectEvmSnapshotQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(request.ProjectId, cancellationToken);
    }

    private async Task<ProjectEvmSnapshot?> LoadAsync(string projectId, CancellationToken cancellationToken)
    {
        var stored = await store.FindAsync(projectId, cancellationToken).ConfigureAwait(false);

        return stored is null ? null : ProjectEvmParser.ParseSnapshot(projectId, stored.Payload, stored.SyncedAtUtc);
    }
}
