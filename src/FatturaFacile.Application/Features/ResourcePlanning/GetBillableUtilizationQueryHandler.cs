// <copyright file="GetBillableUtilizationQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Handler for <see cref="GetBillableUtilizationQuery"/>.
/// </summary>
/// <param name="client">The resource planning API client.</param>
public sealed class GetBillableUtilizationQueryHandler(IResourcePlanningApiClient client) : IRequestHandler<GetBillableUtilizationQuery, UtilizationReport>
{
    /// <inheritdoc/>
    public Task<UtilizationReport> Handle(GetBillableUtilizationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return client.GetBillableUtilizationAsync(request.From, request.To, request.GroupBy, cancellationToken);
    }
}
