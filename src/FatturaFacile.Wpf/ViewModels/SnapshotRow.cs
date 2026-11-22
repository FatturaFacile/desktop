// <copyright file="SnapshotRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One stored weekly capacity snapshot, ready to show.
/// </summary>
/// <param name="Snapshot">The snapshot.</param>
/// <param name="EmployeeName">The employee.</param>
/// <param name="WeekText">The Monday of the week.</param>
/// <param name="NetText">The net available hours.</param>
/// <param name="BillableText">The billable booked hours.</param>
/// <param name="InternalText">The internal booked hours.</param>
/// <param name="UtilizationText">The utilization rate.</param>
internal sealed record SnapshotRow(ResourceCapacitySnapshot Snapshot, string EmployeeName, string WeekText, string NetText, string BillableText, string InternalText, string UtilizationText);
