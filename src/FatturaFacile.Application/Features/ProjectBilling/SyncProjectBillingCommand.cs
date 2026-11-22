// <copyright file="SyncProjectBillingCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ProjectBilling;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectBilling;

/// <summary>
/// Pulls the billing snapshot of a project from the server into the local cache.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
public sealed record SyncProjectBillingCommand(string ProjectId) : IRequest<ProjectBillingSyncResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
