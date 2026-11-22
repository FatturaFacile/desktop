// <copyright file="SlaMetricRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A group of the SLA monitoring table, formatted for display.
/// </summary>
/// <param name="Key">The localized name of the group (queue, priority or contract).</param>
/// <param name="TicketsText">The number of tickets.</param>
/// <param name="MttfrText">The mean time to first response.</param>
/// <param name="MttrText">The mean time to resolution.</param>
/// <param name="FirstResponseText">The first response compliance.</param>
/// <param name="FirstResponseState">The colour state of the first response compliance: Good, Warn or Bad.</param>
/// <param name="ResolutionText">The resolution compliance.</param>
/// <param name="ResolutionState">The colour state of the resolution compliance: Good, Warn or Bad.</param>
/// <param name="CsatText">The satisfaction score with the number of responses.</param>
internal sealed record SlaMetricRow(
    string Key,
    string TicketsText,
    string MttfrText,
    string MttrText,
    string FirstResponseText,
    string FirstResponseState,
    string ResolutionText,
    string ResolutionState,
    string CsatText);
