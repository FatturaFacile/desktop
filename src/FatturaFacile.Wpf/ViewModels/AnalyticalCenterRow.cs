// <copyright file="AnalyticalCenterRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One center of the hierarchy table, formatted for display.
/// </summary>
/// <param name="Id">The center identifier.</param>
/// <param name="Label">The indented code and name.</param>
/// <param name="TypeText">The localized center type.</param>
/// <param name="CategoryText">The localized category.</param>
/// <param name="ManagerText">The center manager.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="IsCostCenter">True for a cost center.</param>
internal sealed record AnalyticalCenterRow(string Id, string Label, string TypeText, string CategoryText, string ManagerText, string StatusText, bool IsCostCenter);
