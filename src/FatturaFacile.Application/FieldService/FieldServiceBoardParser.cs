// <copyright file="FieldServiceBoardParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Reads the `data` of `GET /api/v1/fsm/board` into a <see cref="FieldServiceBoard"/>.
/// </summary>
public static class FieldServiceBoardParser
{
    /// <summary>
    /// Parses the dispatcher board.
    /// </summary>
    /// <param name="json">The `data` object of the answer.</param>
    /// <returns>The board.</returns>
    /// <exception cref="JsonException">The payload is not a board.</exception>
    public static FieldServiceBoard Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = FieldServiceJson.Open(json, JsonValueKind.Object);
        var root = document.RootElement;
        var window = FieldServiceJson.Child(root, "window") ?? default;

        return new FieldServiceBoard(
            FieldServiceJson.Moment(window, "from") ?? DateTimeOffset.MinValue,
            FieldServiceJson.Moment(window, "to") ?? DateTimeOffset.MinValue,
            [.. FieldServiceJson.Items(root, "unassigned").Select(ReadBlock)],
            [.. FieldServiceJson.Items(root, "lanes").Select(ReadLane)]);
    }

    private static FieldServiceLane ReadLane(JsonElement item)
    {
        var van = FieldServiceJson.Child(item, "van") ?? default;

        return new FieldServiceLane(
            FieldServiceJson.Text(item, "technician_id"),
            FieldServiceJson.Text(item, "name"),
            FieldServiceJson.Flag(item, "is_available"),
            FieldServiceJson.Strings(item, "skills"),
            FieldServiceJson.Flag(van, "ready"),
            FieldServiceJson.Whole(van, "distinct_products_in_stock"),
            [.. FieldServiceJson.Items(item, "blocks").Select(ReadBlock)]);
    }

    private static FieldServiceBlock ReadBlock(JsonElement item)
    {
        var site = FieldServiceJson.Child(item, "site") ?? default;

        return new FieldServiceBlock(
            FieldServiceJson.Text(item, "work_order_id"),
            FieldServiceJson.Text(item, "work_order_number"),
            FieldServiceJson.Text(item, "title"),
            FieldServiceJson.Text(item, "work_type"),
            FieldServiceJson.Text(item, "priority"),
            FieldServiceJson.Text(item, "status"),
            FieldServiceJson.Moment(item, "scheduled_start"),
            FieldServiceJson.Moment(item, "scheduled_end"),
            FieldServiceJson.NullableText(site, "address"),
            FieldServiceJson.NullableDouble(site, "latitude"),
            FieldServiceJson.NullableDouble(site, "longitude"),
            FieldServiceJson.Strings(item, "required_skills"),
            FieldServiceJson.Items(item, "required_parts").Count);
    }
}
