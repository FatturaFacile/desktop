// <copyright file="BadDebtProvisionCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// Computes the prudential bad-debt provision: per bucket the open amount times the rate, with default or configured rates.
/// </summary>
public static class BadDebtProvisionCalculator
{
    /// <summary>
    /// Computes the provision.
    /// </summary>
    /// <param name="buckets">The buckets in aging order.</param>
    /// <param name="totals">The open amount per bucket key.</param>
    /// <param name="rates">The configured rates per bucket key, null for the defaults.</param>
    /// <returns>The provision summary.</returns>
    public static ProvisionSummary Calculate(IReadOnlyList<AgingBucket> buckets, IReadOnlyDictionary<string, decimal> totals, IReadOnlyDictionary<string, decimal>? rates)
    {
        ArgumentNullException.ThrowIfNull(buckets);
        ArgumentNullException.ThrowIfNull(totals);

        List<ProvisionLine> lines = [];

        for (var index = 0; index < buckets.Count; index++)
        {
            var bucket = buckets[index];
            var open = ReceivablesMath.Round(totals.GetValueOrDefault(bucket.Key));
            var custom = rates is not null && rates.TryGetValue(bucket.Key, out var configured) && ProvisionRates.IsValid(configured);
            var rate = custom ? rates![bucket.Key] : ProvisionRates.DefaultFor(index);

            lines.Add(new ProvisionLine(bucket.Key, bucket.From, bucket.To, open, rate, ReceivablesMath.Percent(open, rate), !custom));
        }

        var totalOpen = lines.Sum(line => line.OpenAmount);
        var totalProvision = lines.Sum(line => line.Provision);

        return new ProvisionSummary(lines, totalOpen, totalProvision, totalOpen - totalProvision);
    }
}
