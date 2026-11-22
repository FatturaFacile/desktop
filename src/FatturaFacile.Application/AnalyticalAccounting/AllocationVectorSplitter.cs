// <copyright file="AllocationVectorSplitter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// Validates and splits the amount of a posting line over its analytical allocation vectors, by percentage (adding up to exactly
/// 100) or by exact amount (adding up to the line amount). Percentages use the Largest Remainder (Hamilton) method on cents, so the
/// parts always add up to the line amount with no drift.
/// </summary>
public static class AllocationVectorSplitter
{
    private const decimal CentsPerUnit = 100m;
    private const decimal FullPercentage = 100m;

    /// <summary>
    /// Splits a line amount.
    /// </summary>
    /// <param name="lineAmount">The amount of the line.</param>
    /// <param name="vectors">The allocation vectors.</param>
    /// <returns>The validity, the error codes and the amount of each vector.</returns>
    public static AllocationSplitResult Split(decimal lineAmount, IReadOnlyList<AllocationVector> vectors)
    {
        ArgumentNullException.ThrowIfNull(vectors);

        var errors = Validate(vectors);
        if (errors.Count > 0)
        {
            return new AllocationSplitResult(IsValid: false, errors, []);
        }

        var totalCents = ToCents(lineAmount);
        if (vectors.All(v => v.Percentage is not null))
        {
            return vectors.Sum(v => v.Percentage!.Value) != FullPercentage
                ? Invalid("Analytical_VectorPercentSum")
                : new AllocationSplitResult(IsValid: true, [], Allocate(totalCents, vectors.Select(v => v.Percentage!.Value).ToList()).Select(c => c / CentsPerUnit).ToList());
        }

        var cents = vectors.Select(v => ToCents(v.Amount!.Value)).ToList();

        return cents.Sum() == totalCents
            ? new AllocationSplitResult(IsValid: true, [], cents.ConvertAll(c => c / CentsPerUnit))
            : Invalid("Analytical_VectorAmountSum");
    }

    /// <summary>
    /// Largest Remainder (Hamilton) split of cents over non-negative weights; ties go to the earliest weight.
    /// </summary>
    /// <param name="totalCents">The cents to split, negative allowed.</param>
    /// <param name="weights">The non-negative weights.</param>
    /// <returns>The cents of every weight, adding up to <paramref name="totalCents"/>.</returns>
    public static IReadOnlyList<long> Allocate(long totalCents, IReadOnlyList<decimal> weights)
    {
        ArgumentNullException.ThrowIfNull(weights);

        var sum = weights.Sum();
        if (sum <= 0m)
        {
            return weights.Select(_ => 0L).ToList();
        }

        var magnitude = Math.Abs(totalCents);
        var quotas = weights.Select(w => magnitude * w / sum).ToList();
        var shares = quotas.ConvertAll(q => (long)Math.Floor(q));
        var leftover = magnitude - shares.Sum();
        var order = Enumerable.Range(0, weights.Count)
            .OrderByDescending(i => quotas[i] - Math.Floor(quotas[i]))
            .ThenBy(i => i)
            .ToList();
        for (var i = 0L; i < leftover; i++)
        {
            shares[order[(int)(i % order.Count)]]++;
        }

        return totalCents < 0 ? shares.ConvertAll(s => -s) : shares;
    }

    private static long ToCents(decimal amount) => (long)Math.Round(amount * CentsPerUnit, 0, MidpointRounding.AwayFromZero);

    private static AllocationSplitResult Invalid(string error) => new(IsValid: false, [error], []);

    private static List<string> Validate(IReadOnlyList<AllocationVector> vectors)
    {
        var errors = new List<string>();
        if (vectors.Count == 0)
        {
            errors.Add("Analytical_VectorRequired");

            return errors;
        }

        var withPercentage = vectors.Count(v => v.Percentage is not null);
        var withAmount = vectors.Count(v => v.Amount is not null && v.Percentage is null);
        if (withPercentage > 0 && withAmount > 0)
        {
            errors.Add("Analytical_VectorMixedModes");
        }

        if (!vectors.All(v => v.CostCenterId is not null || v.ProfitCenterId is not null || v.ProjectId is not null))
        {
            errors.Add("Analytical_VectorTargetRequired");
        }

        if (errors.Count == 0 && withPercentage + withAmount != vectors.Count)
        {
            errors.Add("Analytical_VectorValueRequired");
        }

        return errors;
    }
}
