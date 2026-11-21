// <copyright file="BomExplosionServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Bom;
using FatturaFacile.Domain.Manufacturing;
using Xunit;

namespace FatturaFacile.UnitTests.Manufacturing;

/// <summary>
/// Tests of the offline multi-level BOM explosion and cost roll-up.
/// </summary>
public sealed class BomExplosionServiceTests
{
    private static readonly Guid ItemA = Guid.NewGuid();
    private static readonly Guid ItemB = Guid.NewGuid();
    private static readonly Guid ItemC = Guid.NewGuid();
    private static readonly Guid ItemD = Guid.NewGuid();
    private static readonly Guid ItemE = Guid.NewGuid();

    /// <summary>
    /// A sub-assembly is exploded through its own BOM, scrap is applied and costs roll up level by level.
    /// </summary>
    [Fact]
    public void Explode_MultiLevel_RollsCostsUp()
    {
        var bomB = Bom(ItemB, "B", Line(ItemD, "D", 3m, cost: 2m), Line(ItemE, "E", 1m, cost: 5m));
        var bomA = Bom(ItemA, "A", Line(ItemB, "B", 2m, cost: 0m), Line(ItemC, "C", 1m, cost: 10m, scrap: 10m));

        var result = BomExplosionService.Explode([bomA, bomB], bomA.Id, quantity: 10m);

        Assert.Equal("B,D,E,C", string.Join(',', result.Rows.Select(r => r.ItemCode)));
        Assert.Equal([1, 2, 2, 1], result.Rows.Select(r => r.Level));
        Assert.Equal(2, result.MaxLevel);

        var b = result.Rows[0];
        Assert.True(b.IsMakeItem);
        Assert.Equal(20m, b.Quantity);
        Assert.Equal(220m, b.RolledUpCost);
        Assert.Equal(11m, b.UnitCost);
        Assert.Equal(120m, result.Rows[1].ExtendedCost);
        Assert.Equal(100m, result.Rows[2].ExtendedCost);
        Assert.Equal(11m, result.Rows[3].Quantity);
        Assert.Equal(110m, result.Rows[3].ExtendedCost);
        Assert.Equal(330m, result.TotalCost);
    }

    /// <summary>
    /// The component quantity is scaled by the BOM base quantity.
    /// </summary>
    [Fact]
    public void Explode_UsesBaseQuantity()
    {
        var bom = Bom(ItemA, "A", Line(ItemC, "C", 5m, cost: 1m));
        bom.BaseQuantity = 10m;

        var result = BomExplosionService.Explode([bom], bom.Id, quantity: 20m);

        Assert.Equal(10m, result.Rows.Single().Quantity);
    }

    /// <summary>
    /// A component whose BOM is not active is treated as a purchased part.
    /// </summary>
    [Fact]
    public void Explode_IgnoresInactiveChildBom()
    {
        var draft = Bom(ItemB, "B", Line(ItemD, "D", 1m, cost: 1m));
        draft.Status = "draft";
        var bom = Bom(ItemA, "A", Line(ItemB, "B", 1m, cost: 7m));

        var result = BomExplosionService.Explode([bom, draft], bom.Id, quantity: 1m);

        Assert.False(result.Rows.Single().IsMakeItem);
        Assert.Equal(7m, result.TotalCost);
    }

    /// <summary>
    /// A cycle between two BOMs is reported instead of looping forever.
    /// </summary>
    [Fact]
    public void Explode_Cycle_Throws()
    {
        var bomA = Bom(ItemA, "A", Line(ItemB, "B", 1m, cost: 1m));
        var bomB = Bom(ItemB, "B", Line(ItemA, "A", 1m, cost: 1m));

        var error = Assert.Throws<InvalidOperationException>(() => BomExplosionService.Explode([bomA, bomB], bomA.Id, 1m));

        Assert.Contains("cycle", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// An unknown BOM and a non-positive quantity are rejected.
    /// </summary>
    [Fact]
    public void Explode_InvalidInput_Throws()
    {
        var bom = Bom(ItemA, "A", Line(ItemC, "C", 1m, cost: 1m));

        _ = Assert.Throws<InvalidOperationException>(() => BomExplosionService.Explode([bom], Guid.NewGuid(), 1m));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => BomExplosionService.Explode([bom], bom.Id, 0m));
    }

    private static BomHeader Bom(Guid itemId, string code, params BomLine[] lines)
    {
        BomHeader header = new() { ItemId = itemId, ItemCode = code, ItemName = code + " name", BomNumber = "BOM-" + code, Revision = "A", Status = "active" };
        var sequence = 10;
        foreach (var line in lines)
        {
            line.Sequence = sequence;
            line.BomHeaderId = header.Id;
            header.Lines.Add(line);
            sequence += 10;
        }

        return header;
    }

    private static BomLine Line(Guid itemId, string code, decimal quantity, decimal cost, decimal scrap = 0m) =>
        new() { ComponentItemId = itemId, ComponentItemCode = code, QuantityPer = quantity, StandardUnitCost = cost, ScrapPercentage = scrap };
}
