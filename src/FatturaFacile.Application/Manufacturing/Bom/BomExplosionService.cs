// <copyright file="BomExplosionService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Bom;

/// <summary>
/// Multi-level BOM explosion and cost roll-up over the locally cached BOMs (works offline).
/// </summary>
public static class BomExplosionService
{
    private const int MaxDepth = 32;
    private const int QuantityDecimals = 6;
    private const int CostDecimals = 4;
    private const string ActiveStatus = "active";

    /// <summary>
    /// Explodes a BOM for a quantity and rolls the component costs up.
    /// </summary>
    /// <param name="boms">All cached BOMs with their lines.</param>
    /// <param name="bomId">The BOM to explode.</param>
    /// <param name="quantity">The quantity of the parent item to produce.</param>
    /// <returns>The rows in depth-first order and the total cost.</returns>
    /// <exception cref="InvalidOperationException">The BOM is unknown, nested too deep or part of a cycle.</exception>
    public static BomExplosionResult Explode(IReadOnlyCollection<BomHeader> boms, Guid bomId, decimal quantity)
    {
        ArgumentNullException.ThrowIfNull(boms);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var root = boms.FirstOrDefault(b => b.Id == bomId)
            ?? throw new InvalidOperationException($"BOM {bomId} is not in the local store.");

        var byItem = boms
            .Where(b => b.ItemId != Guid.Empty && string.Equals(b.Status, ActiveStatus, StringComparison.OrdinalIgnoreCase))
            .GroupBy(b => b.ItemId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(b => b.Revision, StringComparer.Ordinal).First());

        List<BomExplosionRow> rows = [];
        var total = Walk(root, quantity, level: 1, prefix: string.Empty, [root.Id], byItem, rows);
        return new BomExplosionResult(rows, Math.Round(total, 2, MidpointRounding.AwayFromZero), rows.Count == 0 ? 0 : rows.Max(r => r.Level));
    }

    private static decimal Walk(
        BomHeader header,
        decimal parentQuantity,
        int level,
        string prefix,
        HashSet<Guid> stack,
        Dictionary<Guid, BomHeader> byItem,
        List<BomExplosionRow> rows)
    {
        if (level > MaxDepth)
        {
            throw new InvalidOperationException($"BOM {header.BomNumber} is nested deeper than {MaxDepth} levels.");
        }

        var baseQuantity = header.BaseQuantity > 0m ? header.BaseQuantity : 1m;
        var subtotal = 0m;

        foreach (var line in header.Lines.OrderBy(l => l.Sequence))
        {
            var quantity = Math.Round(
                line.QuantityPer / baseQuantity * parentQuantity * (1m + (line.ScrapPercentage / 100m)),
                QuantityDecimals,
                MidpointRounding.AwayFromZero);
            var path = prefix + line.Sequence.ToString(CultureInfo.InvariantCulture);

            if (byItem.TryGetValue(line.ComponentItemId, out var child))
            {
                if (stack.Contains(child.Id))
                {
                    throw new InvalidOperationException($"BOM cycle detected at {header.ItemCode} -> {line.ComponentItemCode}.");
                }

                var index = rows.Count;
                rows.Add(null!);
                _ = stack.Add(child.Id);
                var rolled = Walk(child, quantity, level + 1, path + ".", stack, byItem, rows);
                _ = stack.Remove(child.Id);

                var unit = quantity == 0m ? 0m : Math.Round(rolled / quantity, CostDecimals, MidpointRounding.AwayFromZero);
                rows[index] = new BomExplosionRow(level, path, header.ItemCode, line.ComponentItemCode, child.ItemName, quantity, child.Uom, unit, rolled, rolled, line.IsPhantom, IsMakeItem: true);
                subtotal += rolled;
            }
            else
            {
                var extended = Math.Round(quantity * line.StandardUnitCost, CostDecimals, MidpointRounding.AwayFromZero);
                rows.Add(new BomExplosionRow(level, path, header.ItemCode, line.ComponentItemCode, line.ComponentItemCode, quantity, header.Uom, line.StandardUnitCost, extended, extended, line.IsPhantom, IsMakeItem: false));
                subtotal += extended;
            }
        }

        return subtotal;
    }
}
