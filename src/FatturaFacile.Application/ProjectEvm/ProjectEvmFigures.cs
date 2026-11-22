// <copyright file="ProjectEvmFigures.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// The earned value and profitability figures of a project or of one of its WBS nodes, as computed by the server (Mod 082).
/// Amounts are display values; the accounting stays on the server.
/// </summary>
/// <param name="Bac">The budget at completion.</param>
/// <param name="Committed">The committed cost (open purchase orders).</param>
/// <param name="Actual">The actual cost (AC).</param>
/// <param name="Pv">The planned value.</param>
/// <param name="Ev">The earned value.</param>
/// <param name="Cv">The cost variance (EV - AC).</param>
/// <param name="Sv">The schedule variance (EV - PV).</param>
/// <param name="Cpi">The cost performance index, or null when undefined.</param>
/// <param name="Spi">The schedule performance index, or null when undefined.</param>
/// <param name="Eac">The estimate at completion.</param>
/// <param name="Etc">The estimate to complete.</param>
/// <param name="Vac">The variance at completion (BAC - EAC).</param>
/// <param name="AbsorptionPercent">The budget absorption (AC / BAC) in percent, or null without a budget.</param>
/// <param name="RecognizedRevenue">The recognised revenue.</param>
/// <param name="GrossMargin">The gross margin amount.</param>
/// <param name="GrossMarginPercent">The gross margin in percent, or null without revenue.</param>
/// <param name="ManualEtc">The manual bottom-up ETC, or null when the forecast follows BAC / CPI.</param>
public sealed record ProjectEvmFigures(
    double Bac,
    double Committed,
    double Actual,
    double Pv,
    double Ev,
    double Cv,
    double Sv,
    double? Cpi,
    double? Spi,
    double Eac,
    double Etc,
    double Vac,
    double? AbsorptionPercent,
    double RecognizedRevenue,
    double GrossMargin,
    double? GrossMarginPercent,
    double? ManualEtc)
{
    private const double AmberFrom = 0.9;
    private const double GreenFrom = 1.0;
    private const double AbsorptionLimit = 80.0;

    /// <summary>
    /// Gets the committed plus the actual cost.
    /// </summary>
    public double Exposure => Committed + Actual;

    /// <summary>
    /// Gets the traffic light of the cost performance index.
    /// </summary>
    public ProjectEvmLight CpiLight => LightOf(Cpi);

    /// <summary>
    /// Gets the traffic light of the schedule performance index.
    /// </summary>
    public ProjectEvmLight SpiLight => LightOf(Spi);

    /// <summary>
    /// Gets a value indicating whether more than 80% of the budget is absorbed.
    /// </summary>
    public bool AbsorptionWarning => AbsorptionPercent is > AbsorptionLimit;

    /// <summary>
    /// Gets the number of breached alert thresholds: CPI or SPI below 0.90 and absorption above 80%.
    /// </summary>
    public int Breaches => (CpiLight == ProjectEvmLight.Red ? 1 : 0) + (SpiLight == ProjectEvmLight.Red ? 1 : 0) + (AbsorptionWarning ? 1 : 0);

    /// <summary>
    /// Gets the worst of the two index lights, for a one-dot summary of a row.
    /// </summary>
    public ProjectEvmLight OverallLight => (ProjectEvmLight)Math.Max((int)CpiLight, (int)SpiLight);

    /// <summary>
    /// Gets the traffic light of an index.
    /// </summary>
    /// <param name="index">The index, or null when undefined.</param>
    /// <returns>Green from 1.00, amber from 0.90, red below; green for an undefined index.</returns>
    public static ProjectEvmLight LightOf(double? index)
    {
        return index switch
        {
            null or >= GreenFrom => ProjectEvmLight.Green,
            >= AmberFrom => ProjectEvmLight.Amber,
            _ => ProjectEvmLight.Red,
        };
    }
}
