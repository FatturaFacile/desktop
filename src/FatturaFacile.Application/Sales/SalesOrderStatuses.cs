// <copyright file="SalesOrderStatuses.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sales;

/// <summary>
/// Workflow statuses of a sales order (same vocabulary as web-api).
/// </summary>
public static class SalesOrderStatuses
{
    /// <summary>
    /// The "draft" status.
    /// </summary>
    public const string Draft = "draft";

    /// <summary>
    /// The "confirmed" status.
    /// </summary>
    public const string Confirmed = "confirmed";

    /// <summary>
    /// The "approved" status.
    /// </summary>
    public const string Approved = "approved";

    /// <summary>
    /// The "on_hold" status.
    /// </summary>
    public const string OnHold = "on_hold";

    /// <summary>
    /// The "in_production" status.
    /// </summary>
    public const string InProduction = "in_production";

    /// <summary>
    /// The "in_fulfillment" status.
    /// </summary>
    public const string InFulfillment = "in_fulfillment";

    /// <summary>
    /// The "shipped" status.
    /// </summary>
    public const string Shipped = "shipped";

    /// <summary>
    /// The "completed" status.
    /// </summary>
    public const string Completed = "completed";

    /// <summary>
    /// The "cancelled" status.
    /// </summary>
    public const string Cancelled = "cancelled";

    /// <summary>
    /// The "rejected" status.
    /// </summary>
    public const string Rejected = "rejected";
}
