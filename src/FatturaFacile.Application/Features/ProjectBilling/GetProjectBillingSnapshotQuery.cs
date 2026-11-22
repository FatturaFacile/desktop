// <copyright file="GetProjectBillingSnapshotQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ProjectBilling;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectBilling;

/// <summary>
/// Reads the cached billing snapshot of a project. It never calls the server, so it works offline.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
public sealed record GetProjectBillingSnapshotQuery(string ProjectId) : IRequest<ProjectBillingSnapshot?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
