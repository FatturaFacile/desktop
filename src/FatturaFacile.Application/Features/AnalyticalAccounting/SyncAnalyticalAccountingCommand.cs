// <copyright file="SyncAnalyticalAccountingCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.AnalyticalAccounting;

/// <summary>
/// Sends the entries queued offline and pulls the analytical read models into the local cache.
/// </summary>
/// <param name="From">The first posting date of the reports, YYYY-MM-DD, or null.</param>
/// <param name="To">The last posting date of the reports, YYYY-MM-DD, or null.</param>
public sealed record SyncAnalyticalAccountingCommand(string? From = null, string? To = null) : IRequest<AnalyticalSyncResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
