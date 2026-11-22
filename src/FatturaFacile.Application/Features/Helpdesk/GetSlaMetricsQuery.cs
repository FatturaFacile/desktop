// <copyright file="GetSlaMetricsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Loads the SLA monitoring metrics of a period (MTTFR, MTTR, compliance and CSAT), with the cache as offline fallback.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
/// <param name="GroupBy">The grouping: queue, priority or contract.</param>
public sealed record GetSlaMetricsQuery(DateOnly From, DateOnly To, string GroupBy) : IRequest<HelpdeskLoad<HelpdeskMetrics>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
