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

    public static JsonElement? Child(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;

    public static IEnumerable<JsonElement> Items(JsonElement element, string name) =>
        Child(element, name) is { ValueKind: JsonValueKind.Array } array ? array.EnumerateArray() : [];

    public static string Text(JsonElement element, string name) => NullableText(element, name) ?? string.Empty;

    public static string? NullableText(JsonElement element, string name) => Child(element, name) switch
    {
        { ValueKind: JsonValueKind.String } value => value.GetString() is { Length: > 0 } text ? text : null,
        { ValueKind: JsonValueKind.Number } value => value.GetRawText(),
        _ => null,
    };

    public static decimal Amount(JsonElement element, string name) => NullableAmount(element, name) ?? 0m;

    public static decimal? NullableAmount(JsonElement element, string name) => Child(element, name) switch
    {
        { ValueKind: JsonValueKind.Number } value when value.TryGetDecimal(out var number) => number,
        { ValueKind: JsonValueKind.String } value when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var number) => number,
        _ => null,
    };

    public static int Whole(JsonElement element, string name) => NullableWhole(element, name) ?? 0;

    public static int? NullableWhole(JsonElement element, string name) => NullableAmount(element, name) is { } number ? (int)Math.Truncate(number) : null;

    public static double? NullableDouble(JsonElement element, string name) => NullableAmount(element, name) is { } number ? (double)number : null;

    public static bool Flag(JsonElement element, string name) => Child(element, name) is { ValueKind: JsonValueKind.True };

    public static DateTimeOffset? Moment(JsonElement element, string name) =>
        NullableText(element, name) is { } text && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var moment) ? moment : null;

    public static DateOnly? Day(JsonElement element, string name) =>
        Moment(element, name) is { } moment ? DateOnly.FromDateTime(moment.UtcDateTime) : null;

    public static IReadOnlyList<string> Strings(JsonElement element, string name) =>
        Child(element, name) is { ValueKind: JsonValueKind.Array } array
            ? [.. array.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString() ?? string.Empty)]
            : NoStrings;
}
