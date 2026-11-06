// <copyright file="DepreciationCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Assets;

/// <summary>
/// Computes the yearly depreciation of fixed assets: straight-line on cost (Italian ministerial rates, optionally half
/// rate in the first year) or declining on book value (Swiss practice, double the straight-line rate).
/// </summary>
public static class DepreciationCalculator
{
    /// <summary>
    /// The straight-line method.
    /// </summary>
    public const string StraightLine = "StraightLine";

    /// <summary>
    /// The declining-balance method.
    /// </summary>
    public const string Declining = "Declining";

    /// <summary>
    /// Returns the depreciation of a year.
    /// </summary>
    /// <param name="asset">The asset.</param>
    /// <param name="year">The year.</param>
    /// <param name="accumulatedBefore">The depreciation accumulated before the year.</param>
    /// <returns>The amount (0 when fully depreciated, not yet in service or disposed).</returns>
    public static decimal Amount(FixedAsset asset, int year, decimal accumulatedBefore)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var start = (asset.InServiceDate ?? asset.AcquisitionDate).Year;
        var depreciable = asset.AcquisitionCost - asset.ResidualValue;
        var remaining = depreciable - accumulatedBefore;

        if (year < start || remaining <= 0m || (asset.DisposalDate is { } disposal && disposal.Year < year))
        {
            return 0m;
        }

        var rate = asset.RatePercent / 100m;

        if (asset.HalfRateFirstYear && year == start)
        {
            rate /= 2m;
        }

        var basis = string.Equals(asset.Method, Declining, StringComparison.Ordinal)
            ? asset.AcquisitionCost - accumulatedBefore
            : asset.AcquisitionCost;
        var amount = Math.Round(basis * rate, 2, MidpointRounding.AwayFromZero);

        // A declining balance never reaches zero: the last franc is written off with the last instalment.
        return remaining - amount < 1m ? remaining : Math.Min(amount, remaining);
    }

    /// <summary>
    /// Returns the depreciation plan from the first year until the asset is fully depreciated (max. 50 years).
    /// </summary>
    /// <param name="asset">The asset.</param>
    /// <returns>The yearly amounts and book values.</returns>
    public static IReadOnlyList<(int Year, decimal Amount, decimal BookValue)> Plan(FixedAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        List<(int Year, decimal Amount, decimal BookValue)> plan = [];
        var accumulated = 0m;
        var start = (asset.InServiceDate ?? asset.AcquisitionDate).Year;

        for (var year = start; year < start + 50; year++)
        {
            var amount = Amount(asset, year, accumulated);

            if (amount <= 0m)
            {
                break;
            }

            accumulated += amount;
            plan.Add((year, amount, asset.AcquisitionCost - accumulated));
        }

        return plan;
    }
}
