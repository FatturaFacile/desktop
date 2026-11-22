// <copyright file="SearchResourceCandidatesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ResourcePlanning;

/// <summary>
/// Suggests the employees that fit a skill requirement and period.
/// </summary>
/// <param name="Search">The search criteria.</param>
public sealed record SearchResourceCandidatesQuery(CandidateSearchRequest Search) : IRequest<IReadOnlyList<ResourceCandidate>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
