// <copyright file="HelpdeskMetricGroup.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The SLA figures of one group (queue, priority or contract) of the metrics query.
/// </summary>
/// <param name="Key">The group key.</param>
/// <param name="Tickets">The number of tickets.</param>
/// <param name="MttfrMinutes">The mean time to first response in minutes.</param>
/// <param name="MttrMinutes">The mean time to resolution in minutes.</param>
/// <param name="FirstResponseCompliancePercent">The share of tickets answered within the SLA.</param>
/// <param name="ResolutionCompliancePercent">The share of tickets resolved within the SLA.</param>
/// <param name="Csat">The satisfaction figures.</param>
public sealed record HelpdeskMetricGroup(
    string Key,
    int Tickets,
    double? MttfrMinutes,
    double? MttrMinutes,
    double? FirstResponseCompliancePercent,
    double? ResolutionCompliancePercent,
    HelpdeskCsat Csat);
