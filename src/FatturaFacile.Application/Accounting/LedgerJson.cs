// <copyright file="LedgerJson.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Small readers over the JSON of the General Ledger endpoints.
/// </summary>
internal static class LedgerJson
{
    /// <summary>
    /// Gets the elements of a JSON array (nothing for any other value).
    /// </summary>
    /// <param name="array">The array element.</param>
    /// <returns>The elements.</returns>
    public static List<JsonElement> Items(JsonElement array)
    {
        List<JsonElement> items = [];
        var length = array.ValueKind == JsonValueKind.Array ? array.GetArrayLength() : 0;

        for (var i = 0; i < length; i++)
        {
            items.Add(array[i]);
        }

        return items;
    }

    /// <summary>
    /// Reads a string property, empty when absent or not a string.
    /// </summary>
    /// <param name="element">The object.</param>
    /// <param name="property">The property name.</param>
    /// <returns>The text.</returns>
    public static string Text(JsonElement element, string property) => OptionalText(element, property) ?? string.Empty;

    /// <summary>
    /// Reads an optional string property.
    /// </summary>
    /// <param name="element">The object.</param>
    /// <param name="property">The property name.</param>
    /// <returns>The text, or null when absent, null or not a string.</returns>
    public static string? OptionalText(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    /// <summary>
    /// Reads a boolean property, false when absent.
    /// </summary>
    /// <param name="element">The object.</param>
    /// <param name="property">The property name.</param>
    /// <returns>The flag.</returns>
    public static bool Flag(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.True;

    /// <summary>
    /// Reads a decimal property sent as a JSON number or a numeric string, zero when absent.
    /// </summary>
    /// <param name="element">The object.</param>
    /// <param name="property">The property name.</param>
    /// <returns>The amount.</returns>
    public static decimal Number(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return 0m;
        }

        var parsed = 0m;
        var isNumber = value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out parsed);
        var isText = value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);

        return isNumber || isText ? parsed : 0m;
    }

    /// <summary>
    /// Reads a GUID property, null when absent or malformed.
    /// </summary>
    /// <param name="element">The object.</param>
    /// <param name="property">The property name.</param>
    /// <returns>The identifier.</returns>
    public static Guid? Id(JsonElement element, string property) =>
        Guid.TryParse(OptionalText(element, property), out var id) ? id : null;
}
