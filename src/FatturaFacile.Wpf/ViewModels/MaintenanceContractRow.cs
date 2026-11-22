// <copyright file="MaintenanceContractRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A maintenance contract of the list, formatted for display.
/// </summary>
/// <param name="Id">The contract identifier.</param>
/// <param name="Number">The number.</param>
/// <param name="Title">The title.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="CoverageText">The localized coverage type.</param>
/// <param name="PeriodText">The validity period.</param>
/// <param name="VisitsText">The visits used against the visits included.</param>
/// <param name="CalloutText">The call-out fee.</param>
/// <param name="FrequencyText">The preventive rule.</param>
internal sealed record MaintenanceContractRow(string Id, string Number, string Title, string StatusText, string CoverageText, string PeriodText, string VisitsText, string CalloutText, string FrequencyText);
