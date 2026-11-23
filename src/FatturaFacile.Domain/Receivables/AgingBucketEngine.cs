// <copyright file="AgingBucketEngine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The aging buckets of the receivables with configurable upper bounds. The default bounds 30, 60, 90 and 120 give current,
/// 1_30, 31_60, 61_90, 91_120 and 121_plus, the same keys the web-api uses.
/// </summary>
public sealed class AgingBucketEngine
{
    /// <summary>
    /// The key of what is not yet due.
    /// </summary>
    public const string Current = "current";

    private static readonly int[] DefaultBoundsArray = [30, 60, 90, 120];

    /// <summary>
    /// Initializes a new instance of the <see cref="AgingBucketEngine"/> class with the default bounds.
    /// </summary>
    public AgingBucketEngine()
        : this(DefaultBoundsArray)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AgingBucketEngine"/> class.
    /// </summary>
    /// <param name="bounds">The ascending upper bounds in days, the first at least one.</param>
    public AgingBucketEngine(IReadOnlyList<int> bounds)
    {
        ArgumentNullException.ThrowIfNull(bounds);

        if (!AreValid(bounds))
        {
            throw new ArgumentException("The aging bounds must be strictly ascending and start from at least one day.", nameof(bounds));
        }

        Buckets = Build(bounds);
    }

    /// <summary>
    /// Gets the default upper bounds.
    /// </summary>
    public static IReadOnlyList<int> DefaultBounds => DefaultBoundsArray;

    /// <summary>
    /// Gets the buckets, current first.
    /// </summary>
    public IReadOnlyList<AgingBucket> Buckets { get; }

    /// <summary>
    /// Builds the engine of the buckets a server answer carries; an answer without usable bounds gives the default engine.
    /// </summary>
    /// <param name="buckets">The buckets of the answer.</param>
    /// <returns>The engine.</returns>
    public static AgingBucketEngine FromBuckets(IEnumerable<AgingBucket> buckets)
    {
        ArgumentNullException.ThrowIfNull(buckets);

        int[] bounds = [.. buckets.Select(bucket => bucket.To).OfType<int>().Where(bound => bound > 0)];

        return AreValid(bounds) ? new AgingBucketEngine(bounds) : new AgingBucketEngine();
    }

    /// <summary>
    /// Gets the key of the bucket of an item.
    /// </summary>
    /// <param name="daysPastDue">The days past due, zero or less when not yet due.</param>
    /// <returns>The bucket key.</returns>
    public string KeyFor(int daysPastDue)
    {
        if (daysPastDue <= 0)
        {
            return Current;
        }

        foreach (var bucket in Buckets)
        {
            if (bucket.To is null || daysPastDue <= bucket.To)
            {
                return bucket.Key;
            }
        }

        return Buckets[^1].Key;
    }

    /// <summary>
    /// Sums the open amount of the items per bucket; every bucket is present.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="today">The day of the aging.</param>
    /// <returns>The total per bucket key.</returns>
    public IReadOnlyDictionary<string, decimal> Totals(IEnumerable<ReceivableItem> items, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(items);

        var totals = Buckets.ToDictionary(bucket => bucket.Key, _ => 0m, StringComparer.Ordinal);

        foreach (var item in items.Where(candidate => candidate.IsOpen))
        {
            totals[KeyFor(item.DaysPastDueOn(today))] += item.OpenAmount;
        }

        return totals;
    }

    /// <summary>
    /// Gets the open items of one bucket, the drill-down of an aging figure.
    /// </summary>
    /// <param name="key">The bucket key.</param>
    /// <param name="items">The items.</param>
    /// <param name="today">The day of the aging.</param>
    /// <returns>The items in the bucket.</returns>
    public IReadOnlyList<ReceivableItem> ItemsIn(string key, IEnumerable<ReceivableItem> items, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(items);

        return [.. items.Where(item => item.IsOpen && string.Equals(KeyFor(item.DaysPastDueOn(today)), key, StringComparison.Ordinal))];
    }

    /// <summary>
    /// Gets the keys of the buckets that start after a number of days past due (the over-90 aggregate).
    /// </summary>
    /// <param name="days">The days.</param>
    /// <returns>The keys.</returns>
    public IReadOnlyList<string> KeysOver(int days) => [.. Buckets.Where(bucket => bucket.From > days).Select(bucket => bucket.Key)];

    private static bool AreValid(IReadOnlyList<int> bounds)
    {
        if (bounds.Count == 0 || bounds[0] < 1)
        {
            return false;
        }

        for (var index = 1; index < bounds.Count; index++)
        {
            if (bounds[index] <= bounds[index - 1])
            {
                return false;
            }
        }

        return true;
    }

    private static List<AgingBucket> Build(IReadOnlyList<int> bounds)
    {
        List<AgingBucket> buckets = [new AgingBucket(Current, 0, 0)];
        var from = 1;

        foreach (var bound in bounds)
        {
            buckets.Add(new AgingBucket(from.ToString(System.Globalization.CultureInfo.InvariantCulture) + "_" + bound.ToString(System.Globalization.CultureInfo.InvariantCulture), from, bound));
            from = bound + 1;
        }

        buckets.Add(new AgingBucket(from.ToString(System.Globalization.CultureInfo.InvariantCulture) + "_plus", from, To: null));

        return buckets;
    }
}
