// <copyright file="AnalyticalRunRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One step-down apportionment run of the history, formatted for display.
/// </summary>
/// <param name="PeriodText">The period, YYYY-MM.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="PoolText">The cost to apportion.</param>
/// <param name="AllocatedText">The cost apportioned.</param>
internal sealed record AnalyticalRunRow(string PeriodText, string StatusText, string PoolText, string AllocatedText);
