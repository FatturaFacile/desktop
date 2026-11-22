// <copyright file="AnalyticalCenterNode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// A cost or profit center with its children, as the center tree endpoint returns it.
/// </summary>
/// <param name="Id">The center identifier.</param>
/// <param name="ParentId">The parent identifier, null for a root.</param>
/// <param name="Code">The code, unique in the company.</param>
/// <param name="Name">The name.</param>
/// <param name="CenterType">COST_CENTER or PROFIT_CENTER.</param>
/// <param name="Category">The category (STRUCTURAL, AUXILIARY, PRODUCTION, LOGISTICS, COMMERCIAL or PROFIT).</param>
/// <param name="ManagerName">The center manager, when there is one.</param>
/// <param name="IsActive">Whether the center accepts postings.</param>
/// <param name="Depth">The depth in the tree, 0 for a root.</param>
/// <param name="Children">The child centers.</param>
public sealed record AnalyticalCenterNode(
    string Id,
    string? ParentId,
    string Code,
    string Name,
    string CenterType,
    string Category,
    string? ManagerName,
    bool IsActive,
    int Depth,
    IReadOnlyList<AnalyticalCenterNode> Children);
