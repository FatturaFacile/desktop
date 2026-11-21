// <copyright file="WorkbenchJson.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Sync;

/// <summary>
/// Tolerant readers for the web-api manufacturing JSON (decimals arrive as strings or numbers, optional members may be absent).
/// </summary>
public static class WorkbenchJson
{
    /// <summary>
    /// Copies the elements of a JSON array (empty when the value is not an array).
    /// </summary>
    /// <param name="array">The JSON array.</param>
    /// <returns>The detached elements.</returns>
    public static IReadOnlyList<JsonElement> Items(JsonElement array) =>
        array.ValueKind == JsonValueKind.Array ? array.Deserialize<List<JsonElement>>() ?? [] : [];

    /// <summary>
    /// Reads a string member.
    /// </summary>
    /// <param name="element">The JSON object.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The text, or null when absent or not a string.</returns>
    public static string? StringOrNull(JsonElement element, string name) =>
        TryGet(element, name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    /// <summary>
    /// Reads a decimal member that may be a JSON number or a numeric string.
    /// </summary>
    /// <param name="element">The JSON object.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The value, or zero when absent or invalid.</returns>
    public static decimal DecimalOrZero(JsonElement element, string name) =>
        TryGet(element, name, out var value) ? ToDecimal(value) : 0m;

    /// <summary>
    /// Reads an integer member.
    /// </summary>
    /// <param name="element">The JSON object.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The value, or zero when absent or invalid.</returns>
    public static int IntOrZero(JsonElement element, string name) =>
        TryGet(element, name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsed) ? parsed : 0;

    /// <summary>
    /// Reads a GUID member.
    /// </summary>
    /// <param name="element">The JSON object.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The identifier, or null when absent or invalid.</returns>
    public static Guid? GuidOrNull(JsonElement element, string name) =>
        Guid.TryParse(StringOrNull(element, name), out var parsed) ? parsed : null;

    /// <summary>
    /// Reads a date or ISO-8601 date-time member.
    /// </summary>
    /// <param name="element">The JSON object.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The instant, or null when absent or invalid.</returns>
    public static DateTime? DateOrNull(JsonElement element, string name) =>
        DateTime.TryParse(StringOrNull(element, name), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : null;

    /// <summary>
    /// Maps a server status text to the local enum.
    /// </summary>
    /// <param name="raw">The server status (snake case).</param>
    /// <returns>The status, or null when unknown.</returns>
    public static ProductionOrderStatus? ParseStatus(string? raw) => raw?.Trim().ToUpperInvariant() switch
    {
        "PLANNED" or "DRAFT" => ProductionOrderStatus.Planned,
        "RELEASED" => ProductionOrderStatus.Released,
        "IN_PROGRESS" or "STARTED" => ProductionOrderStatus.InProgress,
        "COMPLETED" => ProductionOrderStatus.Completed,
        "CLOSED" => ProductionOrderStatus.Closed,
        "CANCELLED" or "CANCELED" => ProductionOrderStatus.Cancelled,
        _ => null,
    };

    /// <summary>
    /// Reads the status of a serialized production order.
    /// </summary>
    /// <param name="json">The order JSON (the "data" object).</param>
    /// <returns>The status, or null when unreadable.</returns>
    public static ProductionOrderStatus? ReadOrderStatus(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return ParseStatus(StringOrNull(document.RootElement, "status"));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static decimal ToDecimal(JsonElement value) =>
        decimal.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;

    private static bool TryGet(JsonElement element, string name, out JsonElement value)
    {
        value = default;
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out value);
    }
}
