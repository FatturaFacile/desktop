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
            [.. FieldServiceJson.Items(root, "unassigned").Select(Block)],
            [.. FieldServiceJson.Items(root, "lanes").Select(Lane)]);
    }

    private static FieldServiceLane Lane(JsonElement lane)
    {
        var van = FieldServiceJson.Child(lane, "van") ?? default;

        return new FieldServiceLane(
            FieldServiceJson.Text(lane, "technician_id"),
            FieldServiceJson.Text(lane, "name"),
            FieldServiceJson.Flag(lane, "is_available"),
            FieldServiceJson.Strings(lane, "skills"),
            FieldServiceJson.Flag(van, "ready"),
            FieldServiceJson.Whole(van, "distinct_products_in_stock"),
            [.. FieldServiceJson.Items(lane, "blocks").Select(Block)]);
    }

    private static FieldServiceBlock Block(JsonElement block)
    {
        var site = FieldServiceJson.Child(block, "site") ?? default;

        return new FieldServiceBlock(
            FieldServiceJson.Text(block, "work_order_id"),
            FieldServiceJson.Text(block, "work_order_number"),
            FieldServiceJson.Text(block, "title"),
            FieldServiceJson.Text(block, "work_type"),
            FieldServiceJson.Text(block, "priority"),
            FieldServiceJson.Text(block, "status"),
            FieldServiceJson.Moment(block, "scheduled_start"),
            FieldServiceJson.Moment(block, "scheduled_end"),
            FieldServiceJson.NullableText(site, "address"),
            FieldServiceJson.NullableDouble(site, "latitude"),
            FieldServiceJson.NullableDouble(site, "longitude"),
            FieldServiceJson.Strings(block, "required_skills"),
            FieldServiceJson.Items(block, "required_parts").Count());
    }
}
