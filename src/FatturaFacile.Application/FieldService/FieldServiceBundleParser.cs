// <copyright file="FieldServiceBundleParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Reads the `data` of `GET /api/v1/fsm/technicians/{id}/offline-bundle` into a <see cref="FieldServiceTechnicianBundle"/>.
/// </summary>
public static class FieldServiceBundleParser
{
    /// <summary>
    /// Parses the offline bundle of a technician; the customers and the equipment are folded into the work orders.
    /// </summary>
    /// <param name="json">The `data` object of the answer.</param>
    /// <returns>The bundle.</returns>
    /// <exception cref="JsonException">The payload is not a bundle.</exception>
    public static FieldServiceTechnicianBundle Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Object);
        var root = document.RootElement;
        var technician = FieldServiceJson.Child(root, "technician") ?? default;
        var customers = FieldServiceJson.Items(root, "customers").ToDictionary(c => FieldServiceJson.Text(c, "id"), c => FieldServiceJson.Text(c, "name"), StringComparer.Ordinal);
        var serials = FieldServiceJson.Items(root, "equipment").ToDictionary(e => FieldServiceJson.Text(e, "id"), e => FieldServiceJson.Text(e, "serial_number"), StringComparer.Ordinal);

        return new FieldServiceTechnicianBundle(
            FieldServiceJson.Moment(root, "server_time") ?? DateTimeOffset.MinValue,
            FieldServiceJson.Text(technician, "id"),
            FieldServiceJson.Text(technician, "name"),
            FieldServiceJson.Strings(technician, "skills"),
            [.. FieldServiceJson.Items(root, "work_orders").Select(order => Item(order, customers, serials))],
            FieldServiceJson.Items(root, "van_stock").Count());
    }

    private static FieldServiceBundleItem Item(JsonElement order, Dictionary<string, string> customers, Dictionary<string, string> serials)
    {
        var site = FieldServiceJson.Child(order, "site") ?? default;

        return new FieldServiceBundleItem(
            FieldServiceJson.Text(order, "id"),
            FieldServiceJson.Text(order, "work_order_number"),
            FieldServiceJson.Text(order, "work_type"),
            FieldServiceJson.Text(order, "priority"),
            FieldServiceJson.Text(order, "status"),
            FieldServiceJson.Text(order, "title"),
            FieldServiceJson.Moment(order, "scheduled_start"),
            FieldServiceJson.Moment(order, "scheduled_end"),
            FieldServiceJson.NullableText(site, "address"),
            FieldServiceJson.Strings(order, "required_skills"),
            [.. FieldServiceJson.Items(order, "checklist_template").Select(line => FieldServiceJson.Text(line, "item"))],
            Lookup(customers, FieldServiceJson.NullableText(order, "customer_id")),
            Lookup(serials, FieldServiceJson.NullableText(order, "equipment_passport_id")));
    }

    private static string? Lookup(Dictionary<string, string> map, string? key) =>
        key is not null && map.TryGetValue(key, out var value) ? value : null;
}
