// <copyright file="AllocationRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One allocation of the allocations list, ready to show.
/// </summary>
/// <param name="Allocation">The allocation.</param>
/// <param name="ReferenceText">The project (with WBS node) or the opportunity.</param>
/// <param name="PeriodText">The period.</param>
/// <param name="HoursText">The hours per day and the total.</param>
/// <param name="BookingText">The localized booking type.</param>
/// <param name="IsSoft">Whether the booking is tentative.</param>
/// <param name="IsOverloaded">Whether the booking overloads the employee.</param>
/// <param name="PeakText">The highest weekly load.</param>
internal sealed record AllocationRow(ResourceAllocation Allocation, string ReferenceText, string PeriodText, string HoursText, string BookingText, bool IsSoft, bool IsOverloaded, string PeakText);
