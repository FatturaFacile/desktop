// <copyright file="BomExplosionRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Bom;

/// <summary>
/// One row of a multi-level BOM explosion.
/// </summary>
/// <param name="Level">The BOM level (1 = direct component).</param>
/// <param name="Path">The dotted sequence path.</param>
/// <param name="ParentItemCode">The parent item code.</param>
/// <param name="ItemCode">The component item code.</param>
/// <param name="ItemName">The component description.</param>
/// <param name="Quantity">The required quantity including scrap.</param>
/// <param name="Uom">The unit of measure.</param>
/// <param name="UnitCost">The unit cost (rolled up for make items).</param>
/// <param name="ExtendedCost">The quantity times the unit cost.</param>
/// <param name="RolledUpCost">The cost of the subtree below this row (equals the extended cost for leaves).</param>
/// <param name="IsPhantom">A value indicating whether the line is a phantom.</param>
/// <param name="IsMakeItem">A value indicating whether the component has its own BOM.</param>
public sealed record BomExplosionRow(int Level, string Path, string ParentItemCode, string ItemCode, string ItemName, decimal Quantity, string Uom, decimal UnitCost, decimal ExtendedCost, decimal RolledUpCost, bool IsPhantom, bool IsMakeItem);
