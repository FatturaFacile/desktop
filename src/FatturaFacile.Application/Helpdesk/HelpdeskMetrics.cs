// <copyright file="HelpdeskMetrics.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The SLA monitoring figures of a period, grouped by queue, priority or contract.
/// </summary>
/// <param name="GroupBy">The grouping: queue, priority or contract.</param>
/// <param name="Groups">The groups.</param>
public sealed record HelpdeskMetrics(string GroupBy, IReadOnlyList<HelpdeskMetricGroup> Groups)
{
    /// <summary>
    /// Gets the number of tickets over all groups.
    /// </summary>
    public int TotalTickets => Groups.Sum(group => group.Tickets);
}
