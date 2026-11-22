// <copyright file="FieldServiceJson.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Tolerant readers of the JSON of the Field Service endpoints: the server sends decimals as strings, dates as ISO 8601 and
/// leaves absent what it does not know, so every reader has a neutral default instead of throwing.
/// </summary>
internal static class FieldServiceJson
{
    private static readonly IReadOnlyList<string> NoStrings = [];

    /// <summary>
    /// Parses a payload and checks its root, so a payload of the wrong shape never reaches the cache.
    /// </summary>
    /// <param name="json">The payload.</param>
    /// <param name="kind">The expected root kind (object or array).</param>
    /// <returns>The document, to dispose.</returns>
    /// <exception cref="JsonException">The root is not of the expected kind.</exception>
    public static JsonDocument Open(string json, JsonValueKind kind)
    {
        var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind == kind)
        {
            return document;
        }

        document.Dispose();

        throw new JsonException("The field service payload is not a JSON " + kind.ToString().ToLowerInvariant() + ".");
    }

    /// <summary>
    /// Reads an optional child property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The child, or null when the parent is not an object or the property is absent or null.</returns>
    public static JsonElement? Child(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;

    /// <summary>
    /// Reads the elements of an array property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The elements, empty when the property is absent or not an array.</returns>
    public static IReadOnlyList<JsonElement> Items(JsonElement element, string name) =>
        Child(element, name) is { } child ? Elements(child) : [];

    /// <summary>
    /// Reads the elements of an array.
    /// </summary>
    /// <param name="array">The array element.</param>
    /// <returns>The elements, empty when the element is not an array.</returns>
    public static IReadOnlyList<JsonElement> Elements(JsonElement array)
    {
        var items = new List<JsonElement>();
        if (array.ValueKind == JsonValueKind.Array)
        {
            var count = array.GetArrayLength();
            for (var index = 0; index < count; index++)
            {
                items.Add(array[index]);
            }
        }

        return items;
    }

    /// <summary>
    /// Reads a text property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The text, or an empty string.</returns>
    public static string Text(JsonElement element, string name) => NullableText(element, name) ?? string.Empty;

    /// <summary>
    /// Reads a text property, accepting numbers as their raw text.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The text, or null when absent or empty.</returns>
    public static string? NullableText(JsonElement element, string name) => Child(element, name) switch
    {
        { ValueKind: JsonValueKind.String } value => value.GetString() is { Length: > 0 } text ? text : null,
        { ValueKind: JsonValueKind.Number } value => value.GetRawText(),
        _ => null,
    };

    /// <summary>
    /// Reads a decimal property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The amount, or zero.</returns>
    public static decimal Amount(JsonElement element, string name) => NullableAmount(element, name) ?? 0m;

    /// <summary>
    /// Reads a decimal property sent as a number or as a string.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The amount, or null when absent or not numeric.</returns>
    public static decimal? NullableAmount(JsonElement element, string name) => Child(element, name) switch
    {
        { ValueKind: JsonValueKind.Number } value when value.TryGetDecimal(out var number) => number,
        { ValueKind: JsonValueKind.String } value when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var number) => number,
        _ => null,
    };

    /// <summary>
    /// Reads an integer property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The integer, or zero.</returns>
    public static int Whole(JsonElement element, string name) => NullableWhole(element, name) ?? 0;

    /// <summary>
    /// Reads an integer property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The integer, or null when absent or not numeric.</returns>
    public static int? NullableWhole(JsonElement element, string name) => NullableAmount(element, name) is { } number ? (int)Math.Truncate(number) : null;

    /// <summary>
    /// Reads a floating point property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The number, or null when absent or not numeric.</returns>
    public static double? NullableDouble(JsonElement element, string name) => NullableAmount(element, name) is { } number ? (double)number : null;

    /// <summary>
    /// Reads a boolean property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>True only when the property is the JSON literal true.</returns>
    public static bool Flag(JsonElement element, string name) => Child(element, name) is { ValueKind: JsonValueKind.True };

    /// <summary>
    /// Reads a date-time property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The moment, or null when absent or not a date.</returns>
    public static DateTimeOffset? Moment(JsonElement element, string name) =>
        NullableText(element, name) is { } text && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var moment) ? moment : null;

    /// <summary>
    /// Reads a date property.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The day (UTC), or null when absent or not a date.</returns>
    public static DateOnly? Day(JsonElement element, string name) =>
        Moment(element, name) is { } moment ? DateOnly.FromDateTime(moment.UtcDateTime) : null;

    /// <summary>
    /// Reads an array of strings.
    /// </summary>
    /// <param name="element">The parent element.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The strings, empty when absent or not an array.</returns>
    public static IReadOnlyList<string> Strings(JsonElement element, string name)
    {
        if (Child(element, name) is not { ValueKind: JsonValueKind.Array } array)
        {
            return NoStrings;
        }

        var strings = new List<string>();
        foreach (var item in Elements(array))
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                strings.Add(item.GetString() ?? string.Empty);
            }
        }

        return strings;
    }
}
