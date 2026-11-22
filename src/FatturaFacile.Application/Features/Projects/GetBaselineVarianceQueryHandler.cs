// <copyright file="GetBaselineVarianceQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Projects;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Handles <see cref="GetBaselineVarianceQuery"/>.
/// </summary>
/// <param name="service">The project portfolio service.</param>
public sealed class GetBaselineVarianceQueryHandler(IProjectPortfolioService service) : IRequestHandler<GetBaselineVarianceQuery, BaselineVariance?>
{
    /// <inheritdoc/>
    public Task<BaselineVariance?> Handle(GetBaselineVarianceQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadVarianceAsync(request.ProjectId, request.BaselineId, cancellationToken);
    }
}
