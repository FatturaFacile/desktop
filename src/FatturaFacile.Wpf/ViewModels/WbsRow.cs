// <copyright file="WbsRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One node of the WBS tree grid, formatted for display.
/// </summary>
/// <param name="Id">The node identifier.</param>
/// <param name="Code">The WBS code.</param>
/// <param name="Title">The node title.</param>
/// <param name="Indent">The left margin that shows the depth.</param>
/// <param name="TypeText">The localized node type.</param>
/// <param name="StartText">The planned start.</param>
/// <param name="EndText">The planned end.</param>
/// <param name="HoursText">The formatted planned hours.</param>
/// <param name="CostText">The formatted planned cost.</param>
/// <param name="PercentText">The formatted progress.</param>
/// <param name="FloatText">The total float in days, or a dash.</param>
/// <param name="IsCritical">Whether the node is on the critical path.</param>
/// <param name="IsSummary">Whether the node has children.</param>
internal sealed record WbsRow(
    Guid Id,
    string Code,
    string Title,
    Thickness Indent,
    string TypeText,
    string StartText,
    string EndText,
    string HoursText,
    string CostText,
    string PercentText,
    string FloatText,
    bool IsCritical,
    bool IsSummary)
{
    /// <summary>
    /// Gets the code and the title, for the pickers.
    /// </summary>
    public string Label => Code + " " + Title;
}
