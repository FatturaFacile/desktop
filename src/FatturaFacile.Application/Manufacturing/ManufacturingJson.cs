// <copyright file="ManufacturingJson.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Tolerant readers of the web-api JSON: numbers may arrive as JSON numbers or as strings (decimal columns), and optional fields may
/// be missing or null.
/// </summary>
internal static class ManufacturingJson
{
    /// <summary>
    /// Reads a required-or-empty string.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The string, or an empty string.</returns>
    public static string Str(JsonElement e, string name) => NullableStr(e, name) ?? string.Empty;

    /// <summary>
    /// Reads an optional string.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The string, or null.</returns>
    public static string? NullableStr(JsonElement e, string name) =>
        Find(e, name) is { ValueKind: JsonValueKind.String } v ? v.GetString() : null;

    /// <summary>
    /// Reads a decimal.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <param name="fallback">The value when the property is missing.</param>
    /// <returns>The decimal.</returns>
    public static decimal Dec(JsonElement e, string name, decimal fallback = 0m) => NullableDec(e, name) ?? fallback;

    /// <summary>
    /// Reads an optional decimal.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The decimal, or null.</returns>
    public static decimal? NullableDec(JsonElement e, string name)
    {
        return Find(e, name) switch
        {
            { ValueKind: JsonValueKind.Number } n => n.GetDecimal(),
            { ValueKind: JsonValueKind.String } s when decimal.TryParse(s.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
            _ => null,
        };
    }

    /// <summary>
    /// Reads a double.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The double, or zero.</returns>
    public static double Dbl(JsonElement e, string name) => (double?)NullableDec(e, name) ?? 0d;

    /// <summary>
    /// Reads an integer.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <param name="fallback">The value when the property is missing.</param>
    /// <returns>The integer.</returns>
    public static int Int(JsonElement e, string name, int fallback = 0) =>
        NullableDec(e, name) is { } d ? (int)d : fallback;

    /// <summary>
    /// Reads a boolean (true, false, 1 or 0).
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The boolean.</returns>
    public static bool Bool(JsonElement e, string name) =>
        Find(e, name) is { } v && (v.ValueKind == JsonValueKind.True || (v.ValueKind == JsonValueKind.Number && v.GetDecimal() != 0m));

    /// <summary>
    /// Reads a required identifier.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The identifier.</returns>
    /// <exception cref="JsonException">The identifier is missing or malformed.</exception>
    public static Guid RequiredGuid(JsonElement e, string name) =>
        NullableGuid(e, name) ?? throw new JsonException("The property " + name + " is not a valid identifier.");

    /// <summary>
    /// Reads an optional identifier.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The identifier, or null.</returns>
    public static Guid? NullableGuid(JsonElement e, string name) =>
        Guid.TryParse(NullableStr(e, name), out var id) ? id : null;

    /// <summary>
    /// Reads an optional date or timestamp as UTC.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The instant, or null.</returns>
    public static DateTime? NullableDate(JsonElement e, string name) =>
        DateTimeOffset.TryParse(NullableStr(e, name), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d) ? d.UtcDateTime : null;

    /// <summary>
    /// Reads a required date or timestamp as UTC.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The instant.</returns>
    /// <exception cref="JsonException">The value is missing or malformed.</exception>
    public static DateTime RequiredDate(JsonElement e, string name) =>
        NullableDate(e, name) ?? throw new JsonException("The property " + name + " is not a valid date.");

    /// <summary>
    /// Reads an array property as raw JSON, "[]" when missing.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The raw JSON.</returns>
    public static string RawArray(JsonElement e, string name) =>
        Find(e, name) is { ValueKind: JsonValueKind.Array } v ? v.GetRawText() : "[]";

    /// <summary>
    /// Reads an object property as raw JSON, "{}" when missing.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The raw JSON.</returns>
    public static string RawObject(JsonElement e, string name) =>
        Find(e, name) is { ValueKind: JsonValueKind.Object } v ? v.GetRawText() : "{}";

    /// <summary>
    /// Enumerates an array property.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The elements (none when missing).</returns>
    public static List<JsonElement> Items(JsonElement e, string name) =>
        Find(e, name) is { ValueKind: JsonValueKind.Array } v ? [.. v.EnumerateArray()] : [];

    /// <summary>
    /// Finds a property that is present and not null.
    /// </summary>
    /// <param name="e">The object.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value, or null.</returns>
    public static JsonElement? Find(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null ? v : null;
}
