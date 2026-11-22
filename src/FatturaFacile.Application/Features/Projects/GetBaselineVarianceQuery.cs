// <copyright file="GetBaselineVarianceQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Compares the current plan of a project with one of its baselines.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="BaselineId">The baseline identifier.</param>
public sealed record GetBaselineVarianceQuery(Guid ProjectId, Guid BaselineId) : IRequest<BaselineVariance?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
