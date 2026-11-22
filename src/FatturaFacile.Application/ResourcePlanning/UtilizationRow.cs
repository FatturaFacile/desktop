// <copyright file="UtilizationRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The billable utilization of a resource, department or cost center against its target.
/// </summary>
/// <param name="KeyId">The key id.</param>
/// <param name="KeyName">The key name.</param>
/// <param name="NetAvailableHours">The net available hours.</param>
/// <param name="BillableCommittedHours">The billable committed hours.</param>
/// <param name="BillableBookedHours">The billable booked hours.</param>
/// <param name="InternalBookedHours">The internal booked hours.</param>
/// <param name="TentativeBookedHours">The tentative booked hours.</param>
/// <param name="ActualBillablePercent">The hard billable hours over the net hours, in percent.</param>
/// <param name="ForecastBillablePercent">All billable hours (hard and soft) over the net hours, in percent.</param>
/// <param name="TargetPercent">The target percent.</param>
/// <param name="GapPercent">Actual minus target, in points.</param>
/// <param name="Status">The status.</param>
public sealed record UtilizationRow(
    Guid? KeyId,
    string KeyName,
    decimal NetAvailableHours,
    decimal BillableCommittedHours,
    decimal BillableBookedHours,
    decimal InternalBookedHours,
    decimal TentativeBookedHours,
    double ActualBillablePercent,
    double ForecastBillablePercent,
    double TargetPercent,
    double GapPercent,
    UtilizationStatus Status);
