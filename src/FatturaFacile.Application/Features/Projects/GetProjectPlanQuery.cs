// <copyright file="GetProjectPlanQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Loads the WBS, dependencies, baselines and critical path of a project from the local database.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
public sealed record GetProjectPlanQuery(Guid ProjectId) : IRequest<ProjectPlan?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
