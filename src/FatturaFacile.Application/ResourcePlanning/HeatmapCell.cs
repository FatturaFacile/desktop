// <copyright file="HeatmapCell.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The capacity of one employee in one week or day.
/// </summary>
/// <param name="PeriodStart">The period start.</param>
/// <param name="ContractualHours">The contractual hours.</param>
/// <param name="LeaveAndHolidayHours">The leave and holiday hours.</param>
/// <param name="NetAvailableHours">The net available hours.</param>
/// <param name="HardBookedHours">The hard booked hours.</param>
/// <param name="SoftBookedHours">The soft booked hours.</param>
/// <param name="BillableBookedHours">The billable booked hours.</param>
/// <param name="InternalBookedHours">The internal booked hours.</param>
/// <param name="LoadPercent">The load: hard plus soft over net, in percent.</param>
/// <param name="Band">The band.</param>
/// <param name="IsOverloaded">The is overloaded.</param>
public sealed record HeatmapCell(
    DateOnly PeriodStart,
    decimal ContractualHours,
    decimal LeaveAndHolidayHours,
    decimal NetAvailableHours,
    decimal HardBookedHours,
    decimal SoftBookedHours,
    decimal BillableBookedHours,
    decimal InternalBookedHours,
    double LoadPercent,
    CapacityBand Band,
    bool IsOverloaded);
