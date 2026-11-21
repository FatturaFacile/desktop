// <copyright file="ManufacturingResources.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Sync;

/// <summary>
/// The relative resource routes below /api/v1/manufacturing that take part in the delta pull.
/// </summary>
public static class ManufacturingResources
{
    /// <summary>
    /// The items (their standard cost prices the BOM components).
    /// </summary>
    public const string Items = "items";

    /// <summary>
    /// The work centres.
    /// </summary>
    public const string WorkCenters = "work-centers";

    /// <summary>
    /// The bills of materials (the detail carries the components).
    /// </summary>
    public const string Boms = "boms";

    /// <summary>
    /// The routings (the detail carries the operations).
    /// </summary>
    public const string Routings = "routings";

    /// <summary>
    /// The production orders (the detail carries the operations).
    /// </summary>
    public const string ProductionOrders = "production-orders";

    /// <summary>
    /// The MRP planned orders.
    /// </summary>
    public const string PlannedOrders = "mrp/planned-orders";

    /// <summary>
    /// The cockpit snapshot.
    /// </summary>
    public const string Cockpit = "cockpit";

    /// <summary>
    /// Gets the list resources in pull order (masters before the orders that reference them).
    /// </summary>
    public static IReadOnlyList<string> PullOrder { get; } = [Items, WorkCenters, Boms, Routings, ProductionOrders, PlannedOrders];

    /// <summary>
    /// Tells whether the list rows of a resource lack their children, so the detail must be fetched.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>True when a detail request per changed row is required.</returns>
    public static bool NeedsDetail(string resource) =>
        string.Equals(resource, Boms, StringComparison.Ordinal) || string.Equals(resource, Routings, StringComparison.Ordinal) || string.Equals(resource, ProductionOrders, StringComparison.Ordinal);
}
