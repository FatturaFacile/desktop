// <copyright file="EquipmentHistoryRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A past service report of an asset, formatted for display.
/// </summary>
/// <param name="ReportId">The service report identifier.</param>
/// <param name="ReportNumber">The report number.</param>
/// <param name="DateText">When it was signed.</param>
/// <param name="TypeText">The localized work type.</param>
/// <param name="Summary">The work performed.</param>
/// <param name="LaborText">The labor hours.</param>
/// <param name="MetersText">The meter readings.</param>
/// <param name="PartsText">The parts replaced, marking the covered ones.</param>
internal sealed record EquipmentHistoryRow(string ReportId, string ReportNumber, string DateText, string TypeText, string Summary, string LaborText, string MetersText, string PartsText);
