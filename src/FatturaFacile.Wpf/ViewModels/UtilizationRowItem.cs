// <copyright file="UtilizationRowItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One row of the billable utilization table, ready to show.
/// </summary>
/// <param name="Name">The resource, department or cost center.</param>
/// <param name="NetText">The net available hours.</param>
/// <param name="ActualText">The actual billable share.</param>
/// <param name="ForecastText">The forecast billable share.</param>
/// <param name="TargetText">The target.</param>
/// <param name="GapText">Actual minus target in points.</param>
/// <param name="StatusName">The status: OnTarget, BelowTarget or AboveTarget.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="ActualValue">The actual share limited to 0..100 for the bar.</param>
/// <param name="TargetValue">The target limited to 0..100 for the bar.</param>
internal sealed record UtilizationRowItem(string Name, string NetText, string ActualText, string ForecastText, string TargetText, string GapText, string StatusName, string StatusText, double ActualValue, double TargetValue);
