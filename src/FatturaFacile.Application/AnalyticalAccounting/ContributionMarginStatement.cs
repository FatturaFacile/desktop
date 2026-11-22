// <copyright file="ContributionMarginStatement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The multi-level contribution margin income statement of the management ledger; percentages are on the operating revenues.
/// </summary>
/// <param name="OperatingRevenues">The operating revenues.</param>
/// <param name="DirectVariableCosts">The direct variable costs.</param>
/// <param name="ContributionMargin1">Contribution margin I: revenues minus direct variable costs.</param>
/// <param name="ContributionMargin1Percent">Contribution margin I in percent of the revenues.</param>
/// <param name="DirectFixedCosts">The direct fixed costs.</param>
/// <param name="ContributionMargin2">Contribution margin II: margin I minus direct fixed costs.</param>
/// <param name="ContributionMargin2Percent">Contribution margin II in percent of the revenues.</param>
/// <param name="IndirectCosts">The indirect costs posted directly.</param>
/// <param name="ApportionedCosts">The net costs apportioned by step-down cycles.</param>
/// <param name="OperatingResult">The operating result: margin II minus indirect and apportioned costs.</param>
/// <param name="OperatingResultPercent">The operating result in percent of the revenues.</param>
public sealed record ContributionMarginStatement(
    decimal OperatingRevenues,
    decimal DirectVariableCosts,
    decimal ContributionMargin1,
    decimal ContributionMargin1Percent,
    decimal DirectFixedCosts,
    decimal ContributionMargin2,
    decimal ContributionMargin2Percent,
    decimal IndirectCosts,
    decimal ApportionedCosts,
    decimal OperatingResult,
    decimal OperatingResultPercent)
{
    /// <summary>
    /// Gets a value indicating whether the reported levels follow from the cost blocks (margin I, margin II and the result).
    /// </summary>
    public bool IsConsistent =>
        ContributionMargin1 == OperatingRevenues - DirectVariableCosts
        && ContributionMargin2 == ContributionMargin1 - DirectFixedCosts
        && OperatingResult == ContributionMargin2 - IndirectCosts - ApportionedCosts;
}
