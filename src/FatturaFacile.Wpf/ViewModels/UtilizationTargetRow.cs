// <copyright file="UtilizationTargetRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One utilization target, ready to show.
/// </summary>
/// <param name="Target">The target.</param>
/// <param name="ScopeKey">The key of the scope in the scope list of the editor.</param>
/// <param name="ScopeText">The department, the cost center or the default of the tenant.</param>
/// <param name="PercentText">The target in percent.</param>
internal sealed record UtilizationTargetRow(UtilizationTarget Target, string ScopeKey, string ScopeText, string PercentText);
