// <copyright file="ProcurementParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// Parses the answers of the purchase order fulfilment endpoints (Mod 046). Amounts and quantities arrive as strings and are read as
/// <see cref="decimal"/> with the invariant culture; both the wrapped (`data`) and the bare form of an answer are read.
/// </summary>
public static class ProcurementParser
{
    private static readonly string[] s_nameProperties = ["name", "company_name", "business_name", "display_name", "legal_name"];

    /// <summary>
    /// Parses a list of orders.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The orders.</returns>
    public static IReadOnlyList<PurchaseOrderView> ParseOrders(string json) => [.. Root(json).Data().AsArray().Select(ToOrder)];

    /// <summary>
    /// Parses one order with its lines.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The order.</returns>
    public static PurchaseOrderView ParseOrder(string json) => ToOrder(Root(json).Data());

    /// <summary>
    /// Parses the overdue orders.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The overdue orders.</returns>
    public static IReadOnlyList<OverdueOrderView> ParseOverdue(string json) =>
    [
        .. Root(json).Data().AsArray().Select(row => new OverdueOrderView(
            row.TryGet("order", out var order) ? ToOrder(order) : ToOrder(default),
            row.Int("days_overdue"),
            [.. row.Strings("overdue_line_ids").Select(id => Guid.TryParse(id, out var parsed) ? parsed : Guid.Empty).Where(id => id != Guid.Empty)],
            row.Int("reminder_count"),
            row.InstantOrNull("last_reminder_at"),
            row.Flag("next_reminder_due"))),
    ];

    /// <summary>
    /// Parses the supplier confirmations of an order.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The confirmations.</returns>
    public static IReadOnlyList<SupplierConfirmationView> ParseConfirmations(string json) =>
    [
        .. Root(json).Data().AsArray().Select(row => new SupplierConfirmationView(
            row.GuidOrEmpty("id"),
            row.GuidOrEmpty("purchase_order_id"),
            row.Text("confirmation_number"),
            row.Flag("has_variance"),
            row.Flag("requires_review"),
            row.TextOrNull("notes"),
            row.DateOrNull("confirmed_at") ?? default,
            row.Dec("price_variance_total"),
            row.Dec("quantity_variance_total"),
            [.. row.Elements("lines").Select(ToConfirmationLine)])),
    ];

    /// <summary>
    /// Parses the dispatch log of an order.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The dispatches.</returns>
    public static IReadOnlyList<DispatchView> ParseDispatches(string json) =>
    [
        .. Root(json).Data().AsArray().Select(row => new DispatchView(
            row.GuidOrEmpty("id"),
            row.Text("channel"),
            row.Text("status"),
            row.TextOrNull("recipient"),
            row.TextOrNull("message_reference"),
            row.InstantOrNull("dispatched_at") ?? default)),
    ];

    /// <summary>
    /// Parses the reminder log of an order.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The reminders.</returns>
    public static IReadOnlyList<ReminderView> ParseReminders(string json) =>
    [
        .. Root(json).Data().AsArray().Select(row => new ReminderView(
            row.GuidOrEmpty("id"),
            row.Int("level"),
            row.Text("channel"),
            row.Text("trigger"),
            row.Int("days_overdue"),
            row.InstantOrNull("sent_at") ?? default)),
    ];

    /// <summary>
    /// Parses a list of dropshipments.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The dropshipments.</returns>
    public static IReadOnlyList<DropshipmentView> ParseDropships(string json) => [.. Root(json).Data().AsArray().Select(ToDropship)];

    /// <summary>
    /// Parses one dropshipment.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The dropshipment.</returns>
    public static DropshipmentView ParseDropship(string json) => ToDropship(Root(json).Data());

    /// <summary>
    /// Parses the pricing preview.
    /// </summary>
    /// <param name="json">The body of the answer.</param>
    /// <returns>The computed amounts.</returns>
    public static PricingPreview ParsePricing(string json)
    {
        var data = Root(json).Data();

        return new PricingPreview(
            [.. data.Elements("discount_cascade").Select(step => Number(step) ?? 0m)],
            data.Dec("effective_discount_percent"),
            data.Dec("net_unit_price"),
            data.Dec("line_net_amount"),
            data.Dec("line_tax_amount"),
            data.Dec("line_total_amount"),
            data.Dec("quantity_base"),
            data.Dec("net_price_per_base_unit"),
            data.Dec("line_net_amount_company_currency"));
    }

    private static PurchaseOrderView ToOrder(JsonElement row)
    {
        var supplier = row.TryGet("supplier", out var found) ? found : default;
        var name = s_nameProperties.Select(property => supplier.TextOrNull(property)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

        return new PurchaseOrderView(
            row.GuidOrEmpty("id"),
            row.Text("po_number"),
            row.GuidOrNull("supplier_id"),
            name,
            row.Text("status"),
            row.DateOrNull("order_date"),
            row.DateOrNull("expected_delivery_date"),
            row.DateOrNull("promised_delivery_date"),
            row.Text("currency"),
            row.Dec("total_amount"),
            row.Flag("is_dropship"),
            row.GuidOrNull("sales_order_id"),
            row.Int("reminder_count"),
            row.InstantOrNull("last_reminder_at"),
            row.TextOrNull("supplier_confirmation_number"),
            row.TextOrNull("dispatch_channel"),
            [.. row.Elements("lines").Select(ToLine)]);
    }

    private static PurchaseOrderLineView ToLine(JsonElement row) => new(
        row.GuidOrEmpty("id"),
        row.Int("line_number"),
        row.Text("description"),
        row.Dec("quantity_ordered"),
        row.Dec("quantity_received"),
        row.Text("uom"),
        row.Text("base_uom"),
        row.Factor(),
        row.Dec("quantity_base_ordered"),
        row.Dec("quantity_base_received"),
        row.DecimalOrNull("quantity_confirmed"),
        [.. row.Elements("discount_cascade").Select(step => Number(step) ?? 0m)],
        row.Dec("discount_percent_effective"),
        row.DateOrNull("promised_delivery_date"),
        row.DateOrNull("confirmed_delivery_date"),
        row.Dec("unit_price"),
        row.Dec("line_net_amount"));

    private static ConfirmationLineView ToConfirmationLine(JsonElement row) => new(
        row.GuidOrEmpty("purchase_order_line_id"),
        row.Dec("ordered_quantity"),
        row.Dec("confirmed_quantity"),
        row.Dec("ordered_unit_price"),
        row.Dec("confirmed_unit_price"),
        row.DateOrNull("requested_delivery_date"),
        row.DateOrNull("confirmed_delivery_date"),
        row.Dec("quantity_variance"),
        row.Dec("quantity_variance_base"),
        row.Dec("price_variance"),
        row.Dec("price_variance_percent"),
        row.Dec("line_total_variance"),
        row.Int("delivery_delay_days"),
        row.TextOrNull("variance_reason"));

    private static DropshipmentView ToDropship(JsonElement row)
    {
        var street = (row.Text("recipient_street") + " " + row.Text("recipient_house_number")).Trim();
        var town = (row.Text("recipient_postal_code") + " " + row.Text("recipient_city")).Trim();
        var address = string.Join(", ", new[] { street, town, row.Text("recipient_country_code") }.Where(part => part.Length > 0));

        return new DropshipmentView(
            row.GuidOrEmpty("id"),
            row.GuidOrEmpty("purchase_order_id"),
            row.GuidOrNull("sales_order_id"),
            row.Text("status"),
            row.Text("recipient_name"),
            address,
            row.TextOrNull("ddt_number"),
            row.InstantOrNull("ddt_issued_at"),
            row.TextOrNull("supplier_shipment_reference"),
            row.TextOrNull("delivery_proof_reference"),
            row.Flag("billing_authorized"),
            row.TextOrNull("billing_authorization_reference"),
            row.InstantOrNull("delivered_at"),
            [.. row.Elements("virtual_stock_movements").Select(ToMovement)]);
    }

    private static DropshipMovementView ToMovement(JsonElement movement) => new(
        movement.GuidOrEmpty("purchase_order_line_id"),
        movement.Text("direction"),
        movement.Dec("quantity_base"),
        movement.Text("base_uom"),
        movement.Dec("unit_cost_base"),
        movement.Dec("valued_amount"),
        movement.InstantOrNull("moved_at") ?? default);

    private static JsonElement Root(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using var document = JsonDocument.Parse(json);

        return document.RootElement.Clone();
    }

    private static JsonElement Data(this JsonElement root) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) ? data : root;

    private static bool TryGet(this JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out value) && value.ValueKind != JsonValueKind.Null)
        {
            return true;
        }

        value = default;

        return false;
    }

    private static IEnumerable<JsonElement> AsArray(this JsonElement element) =>
        element.ValueKind == JsonValueKind.Array ? element.EnumerateArray() : [];

    private static IEnumerable<JsonElement> Elements(this JsonElement element, string name) =>
        element.TryGet(name, out var value) ? value.AsArray() : [];

    private static IEnumerable<string> Strings(this JsonElement element, string name) =>
        element.Elements(name).Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString() ?? string.Empty);

    private static string Text(this JsonElement element, string name) => element.TextOrNull(name) ?? string.Empty;

    private static string? TextOrNull(this JsonElement element, string name)
    {
        if (!element.TryGet(name, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
    }

    private static decimal? Number(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Number => value.TryGetDecimal(out var number) ? number : null,
        JsonValueKind.String => decimal.TryParse(value.GetString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
        _ => null,
    };

    private static decimal? DecimalOrNull(this JsonElement element, string name) => element.TryGet(name, out var value) ? Number(value) : null;

    private static decimal Dec(this JsonElement element, string name) => element.DecimalOrNull(name) ?? 0m;

    private static decimal Factor(this JsonElement element)
    {
        var factor = element.Dec("uom_conversion_factor");

        return factor > 0m ? factor : 1m;
    }

    private static int Int(this JsonElement element, string name) => element.DecimalOrNull(name) is { } number ? (int)number : 0;

    private static bool Flag(this JsonElement element, string name)
    {
        if (!element.TryGet(name, out var value))
        {
            return false;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.Number => value.TryGetDecimal(out var number) && number != 0m,
            JsonValueKind.String => string.Equals(value.GetString(), "1", StringComparison.Ordinal) || string.Equals(value.GetString(), "true", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static DateOnly? DateOrNull(this JsonElement element, string name)
    {
        var text = element.TextOrNull(name);

        return text is { Length: >= 10 } && DateOnly.TryParseExact(text[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) ? day : null;
    }

    private static DateTime? InstantOrNull(this JsonElement element, string name)
    {
        var text = element.TextOrNull(name);

        return !string.IsNullOrWhiteSpace(text) && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var instant)
            ? DateTime.SpecifyKind(instant, DateTimeKind.Utc)
            : null;
    }

    private static Guid? GuidOrNull(this JsonElement element, string name) =>
        Guid.TryParse(element.TextOrNull(name), out var id) ? id : null;

    private static Guid GuidOrEmpty(this JsonElement element, string name) => element.GuidOrNull(name) ?? Guid.Empty;
}
