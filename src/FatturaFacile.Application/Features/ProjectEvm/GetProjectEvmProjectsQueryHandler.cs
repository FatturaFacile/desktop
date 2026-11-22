// <copyright file="GetProjectEvmProjectsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectEvm;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectEvm;

/// <summary>
/// Handles <see cref="GetProjectEvmProjectsQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetProjectEvmProjectsQueryHandler(IProjectEvmSnapshotStore store) : IRequestHandler<GetProjectEvmProjectsQuery, IReadOnlyList<ProjectEvmProject>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<ProjectEvmProject>> Handle(GetProjectEvmProjectsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ProjectEvmProject>> LoadAsync(CancellationToken cancellationToken)
    {
        var json = await store.FindProjectsAsync(cancellationToken).ConfigureAwait(false);

        return ProjectEvmParser.ParseProjects(json);
    }
}
