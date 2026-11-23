// <copyright file="ProcurementRoutes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The routes of the purchase order fulfilment endpoints of the web-api (/api/v1/procurement, Mod 046).
/// </summary>
public static class ProcurementRoutes
{
    /// <summary>
    /// The route of the purchase orders.
    /// </summary>
    public const string Orders = "procurement/purchase-orders";

    /// <summary>
    /// The route of the overdue orders.
    /// </summary>
    public const string Overdue = Orders + "/overdue";

    /// <summary>
    /// The route of the pricing preview.
    /// </summary>
    public const string PricingPreview = Orders + "/pricing-preview";

    /// <summary>
    /// The route of the dropshipments.
    /// </summary>
    public const string Dropships = "procurement/dropshipments";

    /// <summary>
    /// The key prefix of the order list in the cache.
    /// </summary>
    public const string OrdersPrefix = Orders + "?";

    /// <summary>
    /// The key prefix of the dropshipment list in the cache.
    /// </summary>
    public const string DropshipsPrefix = Dropships + "?";

    /// <summary>
    /// Gets the route of the order list.
    /// </summary>
    /// <param name="status">The wire status to filter by, or null.</param>
    /// <returns>The route with its query.</returns>
    public static string OrderList(string? status) =>
        OrdersPrefix + "per_page=100" + (string.IsNullOrWhiteSpace(status) ? string.Empty : "&status=" + Uri.EscapeDataString(status));

    /// <summary>
    /// Gets the route of one order.
    /// </summary>
    /// <param name="id">The order.</param>
    /// <returns>The route.</returns>
    public static string Order(Guid id) => Orders + "/" + Id(id);

    /// <summary>
    /// Gets the route of the dispatches of an order (read and write the dispatch).
    /// </summary>
    /// <param name="id">The order.</param>
    /// <returns>The route.</returns>
    public static string Dispatches(Guid id) => Order(id) + "/dispatches";

    /// <summary>
    /// Gets the route that transmits an order.
    /// </summary>
    /// <param name="id">The order.</param>
    /// <returns>The route.</returns>
    public static string Dispatch(Guid id) => Order(id) + "/dispatch";

    /// <summary>
    /// Gets the route of the supplier confirmations of an order (read and write).
    /// </summary>
    /// <param name="id">The order.</param>
    /// <returns>The route.</returns>
    public static string Confirmations(Guid id) => Order(id) + "/confirmations";

    /// <summary>
    /// Gets the route of the reminders of an order (read and write).
    /// </summary>
    /// <param name="id">The order.</param>
    /// <returns>The route.</returns>
    public static string Reminders(Guid id) => Order(id) + "/reminders";

    /// <summary>
    /// Gets the route of the dropshipment list.
    /// </summary>
    /// <param name="status">The wire status to filter by, or null.</param>
    /// <returns>The route with its query.</returns>
    public static string DropshipList(string? status) =>
        DropshipsPrefix + "per_page=100" + (string.IsNullOrWhiteSpace(status) ? string.Empty : "&status=" + Uri.EscapeDataString(status));

    /// <summary>
    /// Gets the route of one dropshipment.
    /// </summary>
    /// <param name="id">The dropshipment.</param>
    /// <returns>The route.</returns>
    public static string Dropship(Guid id) => Dropships + "/" + Id(id);

    /// <summary>
    /// Gets the route that issues the transport document of a dropshipment.
    /// </summary>
    /// <param name="id">The dropshipment.</param>
    /// <returns>The route.</returns>
    public static string DropshipDdt(Guid id) => Dropship(id) + "/ddt";

    /// <summary>
    /// Gets the route that records a delivery of a dropshipment.
    /// </summary>
    /// <param name="id">The dropshipment.</param>
    /// <returns>The route.</returns>
    public static string DropshipDeliveries(Guid id) => Dropship(id) + "/deliveries";

    private static string Id(Guid value) => value.ToString("D", CultureInfo.InvariantCulture);
}
