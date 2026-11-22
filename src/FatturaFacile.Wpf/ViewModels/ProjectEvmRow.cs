// <copyright file="ProjectEvmRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One row of the WBS cost table: the project total or a node, with its figures already formatted for the active culture.
/// </summary>
/// <param name="Code">The WBS code.</param>
/// <param name="Title">The title.</param>
/// <param name="Depth">The depth in the WBS (0 for the project and the top-level nodes).</param>
/// <param name="HasFigures">Whether the node has a snapshot.</param>
/// <param name="Bac">The budget at completion.</param>
/// <param name="Committed">The committed cost.</param>
/// <param name="Actual">The actual cost.</param>
/// <param name="Exposure">The total exposure.</param>
/// <param name="Ev">The earned value.</param>
/// <param name="Pv">The planned value.</param>
/// <param name="Cv">The cost variance.</param>
/// <param name="Sv">The schedule variance.</param>
/// <param name="Cpi">The cost performance index.</param>
/// <param name="Spi">The schedule performance index.</param>
/// <param name="Eac">The estimate at completion.</param>
/// <param name="Etc">The estimate to complete (marked when it is a manual estimate).</param>
/// <param name="Vac">The variance at completion.</param>
/// <param name="Margin">The gross margin and its percentage.</param>
/// <param name="CpiLight">The light of the CPI (Green, Amber or Red).</param>
/// <param name="SpiLight">The light of the SPI (Green, Amber or Red).</param>
internal sealed record ProjectEvmRow(
    string Code,
    string Title,
    int Depth,
    bool HasFigures,
    string Bac,
    string Committed,
    string Actual,
    string Exposure,
    string Ev,
    string Pv,
    string Cv,
    string Sv,
    string Cpi,
    string Spi,
    string Eac,
    string Etc,
    string Vac,
    string Margin,
    string CpiLight,
    string SpiLight)
{
    private const double IndentPerLevel = 18.0;

    /// <summary>
    /// Gets the left margin that indents the title by its depth.
    /// </summary>
    public System.Windows.Thickness Indent => new(Depth * IndentPerLevel, 0, 0, 0);
}
