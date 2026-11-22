// <copyright file="GetReceivablesDashboardQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Loads the receivables dashboard (KPIs and blocked customers); offline, the last cached one.
/// </summary>
/// <param name="Currency">The ISO currency, null for the company currency.</param>
public sealed record GetReceivablesDashboardQuery(string? Currency) : IRequest<ReceivablesLoad<ReceivablesDashboard>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
