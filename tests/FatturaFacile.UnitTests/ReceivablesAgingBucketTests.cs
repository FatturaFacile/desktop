// <copyright file="ReceivablesAgingBucketTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Domain.Receivables;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the aging bucket engine: the contract keys, the boundary days, configurable bounds, totals and the drill-down.
/// </summary>
public sealed class ReceivablesAgingBucketTests
{
    private static readonly DateOnly Today = new(2027, 6, 30);
    private static readonly Guid CustomerId = new(0xC0C0C0C0, 0xC0C0, 0xC0C0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0);

    /// <summary>
    /// The default bounds give the six keys of the contract, with the boundary days on the lower bucket.
    /// </summary>
    /// <param name="days">The days past due.</param>
    /// <param name="expected">The expected bucket key.</param>
    [Theory]
    [InlineData(-5, "current")]
    [InlineData(0, "current")]
    [InlineData(1, "1_30")]
    [InlineData(30, "1_30")]
    [InlineData(31, "31_60")]
    [InlineData(60, "31_60")]
    [InlineData(61, "61_90")]
    [InlineData(90, "61_90")]
    [InlineData(91, "91_120")]
    [InlineData(120, "91_120")]
    [InlineData(121, "121_plus")]
    [InlineData(900, "121_plus")]
    public void KeyFor_DefaultBounds_PutsTheBoundariesInTheRightBucket(int days, string expected)
    {
        var engine = new AgingBucketEngine();

        Assert.Equal(expected, engine.KeyFor(days));
    }

    /// <summary>
    /// The default engine exposes current, 1_30, 31_60, 61_90, 91_120 and 121_plus in that order.
    /// </summary>
    [Fact]
    public void Buckets_DefaultBounds_AreCurrentAndFiveRanges()
    {
        var engine = new AgingBucketEngine();

        Assert.Equal(["current", "1_30", "31_60", "61_90", "91_120", "121_plus"], engine.Buckets.Select(bucket => bucket.Key), StringComparer.Ordinal);
        Assert.Equal(new AgingBucket("31_60", 31, 60), engine.Buckets[2]);
        Assert.Null(engine.Buckets[^1].To);
        Assert.Equal(0, engine.Buckets[0].To);
    }

    /// <summary>
    /// Configured bounds change the ranges and the keys.
    /// </summary>
    [Fact]
    public void KeyFor_CustomBounds_UsesTheConfiguredRanges()
    {
        var engine = new AgingBucketEngine([15, 45]);

        Assert.Equal(["current", "1_15", "16_45", "46_plus"], engine.Buckets.Select(bucket => bucket.Key), StringComparer.Ordinal);
        Assert.Equal("1_15", engine.KeyFor(15));
        Assert.Equal("16_45", engine.KeyFor(16));
        Assert.Equal("16_45", engine.KeyFor(45));
        Assert.Equal("46_plus", engine.KeyFor(46));
    }

    /// <summary>
    /// Bounds that are empty, start below one day or do not strictly ascend are refused.
    /// </summary>
    [Fact]
    public void Constructor_InvalidBounds_Throws()
    {
        _ = Assert.Throws<ArgumentException>(() => new AgingBucketEngine([]));
        _ = Assert.Throws<ArgumentException>(() => new AgingBucketEngine([0, 30]));
        _ = Assert.Throws<ArgumentException>(() => new AgingBucketEngine([30, 30]));
        _ = Assert.Throws<ArgumentException>(() => new AgingBucketEngine([60, 30]));
    }

    /// <summary>
    /// The engine built from the buckets of an answer has the same keys; an answer without bounds gives the defaults.
    /// </summary>
    [Fact]
    public void FromBuckets_ServerBuckets_ReproduceTheSameEngine()
    {
        var server = new AgingBucketEngine([10, 20, 40]);

        var rebuilt = AgingBucketEngine.FromBuckets(server.Buckets);
        var fallback = AgingBucketEngine.FromBuckets([]);

        Assert.Equal(server.Buckets.Select(bucket => bucket.Key), rebuilt.Buckets.Select(bucket => bucket.Key), StringComparer.Ordinal);
        Assert.Equal(new AgingBucketEngine().Buckets.Select(bucket => bucket.Key), fallback.Buckets.Select(bucket => bucket.Key), StringComparer.Ordinal);
    }

    /// <summary>
    /// Totals add the open amounts per bucket, every key is present, and paid or written-off items count for nothing.
    /// </summary>
    [Fact]
    public void Totals_SumsTheOpenAmountPerBucket_AndIgnoresClosedItems()
    {
        var engine = new AgingBucketEngine();
        ReceivableItem[] items =
        [
            Item(100m, -10),
            Item(200m, 5),
            Item(50m, 30),
            Item(70m, 31),
            Item(10m, 130),
            Item(999m, 45, ReceivableStatus.WrittenOff),
            Item(0m, 45, ReceivableStatus.Paid, original: 500m),
        ];

        var totals = engine.Totals(items, Today);

        Assert.Equal(6, totals.Count);
        Assert.Equal(100m, totals["current"]);
        Assert.Equal(250m, totals["1_30"]);
        Assert.Equal(70m, totals["31_60"]);
        Assert.Equal(0m, totals["61_90"]);
        Assert.Equal(0m, totals["91_120"]);
        Assert.Equal(10m, totals["121_plus"]);
    }

    /// <summary>
    /// The drill-down of a bucket returns only the open items that fall in it.
    /// </summary>
    [Fact]
    public void ItemsIn_ReturnsTheDrillDownOfABucket()
    {
        var engine = new AgingBucketEngine();
        var inBucket = Item(70m, 31);
        ReceivableItem[] items = [Item(100m, 5), inBucket, Item(10m, 130), Item(5m, 40, ReceivableStatus.WrittenOff)];

        var found = engine.ItemsIn("31_60", items, Today);

        Assert.Same(inBucket, Assert.Single(found));
    }

    /// <summary>
    /// The over-90 aggregate is made of the buckets that start after day 90.
    /// </summary>
    [Fact]
    public void KeysOver_Ninety_ReturnsTheBucketsBeyondNinetyDays() =>
        Assert.Equal(["91_120", "121_plus"], new AgingBucketEngine().KeysOver(90), StringComparer.Ordinal);

    /// <summary>
    /// An item that is not yet due has no days past due, and a due item counts calendar days.
    /// </summary>
    [Fact]
    public void DaysPastDueOn_NotYetDue_IsZero()
    {
        Assert.Equal(0, Item(1m, -3).DaysPastDueOn(Today));
        Assert.Equal(0, Item(1m, 0).DaysPastDueOn(Today));
        Assert.Equal(12, Item(1m, 12).DaysPastDueOn(Today));
    }

    private static ReceivableItem Item(decimal open, int daysPastDue, ReceivableStatus status = ReceivableStatus.Open, decimal? original = null) =>
        new(Guid.NewGuid(), CustomerId, null, 1, "invoice", "INV-1", "EUR", original ?? open, open, Today.AddDays(-daysPastDue - 30), Today.AddDays(-daysPastDue), null, status, Math.Max(0, daysPastDue), null, []);
}
