// <copyright file="SetUtilizationTargetCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="SetUtilizationTargetCommand"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class SetUtilizationTargetCommandHandler(IResourcePlanningApiClient client) : IRequestHandler<SetUtilizationTargetCommand, PlanningCommandOutcome<UtilizationTarget>>
{
    /// <inheritdoc/>
    public Task<PlanningCommandOutcome<UtilizationTarget>> Handle(SetUtilizationTargetCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.SetUtilizationTargetAsync(request.Target, cancellationToken);
    }
}
