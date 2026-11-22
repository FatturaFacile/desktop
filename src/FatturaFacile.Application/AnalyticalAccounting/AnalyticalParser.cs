// <copyright file="AnalyticalParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// Turns the `data` payloads of the analytical endpoints into the typed read models. A payload that does not have the expected
/// shape throws a <see cref="JsonException"/>, so a bad answer never replaces a good cache.
/// </summary>
public static class AnalyticalParser
{
    /// <summary>
    /// Parses the center tree.
    /// </summary>
    /// <param name="json">The `data` array of the tree endpoint.</param>
    /// <returns>The root centers with their children.</returns>
    /// <exception cref="JsonException">The payload is not an array.</exception>
    public static IReadOnlyList<AnalyticalCenterNode> ParseCenters(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.ValueKind == JsonValueKind.Array
            ? ParseArray(document.RootElement, ParseNode)
            : throw new JsonException("The center tree must be an array.");
    }

    /// <summary>
    /// Flattens a tree depth first.
    /// </summary>
    /// <param name="roots">The root centers.</param>
    /// <returns>Every center, parents before children.</returns>
    public static IReadOnlyList<AnalyticalCenterNode> Flatten(IReadOnlyList<AnalyticalCenterNode> roots)
    {
        ArgumentNullException.ThrowIfNull(roots);

        return [.. roots.SelectMany(r => new[] { r }.Concat(Flatten(r.Children)))];
    }

    /// <summary>
    /// Parses the contribution margin statement.
    /// </summary>
    /// <param name="json">The `data` object of the report (with a `statement` property).</param>
    /// <returns>The statement.</returns>
    /// <exception cref="JsonException">The payload has no statement.</exception>
    public static ContributionMarginStatement ParseMargin(string json)
    {
        using var document = JsonDocument.Parse(json);

        return !TryGetMember(document.RootElement, "statement", out var s)
            ? throw new JsonException("The margin report has no statement.")
            : new ContributionMarginStatement(
                Money(s, "operating_revenues"),
                Money(s, "direct_variable_costs"),
                Money(s, "contribution_margin_1"),
                Money(s, "contribution_margin_1_pct"),
                Money(s, "direct_fixed_costs"),
                Money(s, "contribution_margin_2"),
                Money(s, "contribution_margin_2_pct"),
                Money(s, "indirect_costs"),
                Money(s, "apportioned_costs"),
                Money(s, "operating_result"),
                Money(s, "operating_result_pct"));
    }

    /// <summary>
    /// Parses the direct-versus-apportioned comparison.
    /// </summary>
    /// <param name="json">The `data` object of the report (with a `rows` array).</param>
    /// <returns>The rows.</returns>
    /// <exception cref="JsonException">The payload has no rows array.</exception>
    public static IReadOnlyList<DirectVsApportionedRow> ParseComparison(string json)
    {
        using var document = JsonDocument.Parse(json);

        return !TryGetMember(document.RootElement, "rows", out var rows) || rows.ValueKind != JsonValueKind.Array
            ? throw new JsonException("The comparison has no rows.")
            : ParseArray(rows, ParseComparisonRow);
    }

    /// <summary>
    /// Parses the run history.
    /// </summary>
    /// <param name="json">The `data` array of the runs endpoint.</param>
    /// <returns>The runs, in the order of the server.</returns>
    /// <exception cref="JsonException">The payload is not an array.</exception>
    public static IReadOnlyList<ApportionmentRunSummary> ParseRuns(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.ValueKind == JsonValueKind.Array
            ? ParseArray(document.RootElement, ParseRun)
            : throw new JsonException("The run history must be an array.");
    }

    private static bool TryGetMember(JsonElement element, string name, out JsonElement value)
    {
        value = default;

        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out value);
    }

    private static List<T> ParseArray<T>(JsonElement array, Func<JsonElement, T> map)
    {
        var result = new List<T>();
        for (var index = 0; index < array.GetArrayLength(); index++)
        {
            result.Add(map(array[index]));
        }

        return result;
    }

    private static DirectVsApportionedRow ParseComparisonRow(JsonElement r) => new(
        Text(r, "center_id"),
        Text(r, "code"),
        Text(r, "name"),
        r.TryGetProperty("depth", out var depth) ? depth.GetInt32() : 0,
        Money(r, "direct_cost"),
        Money(r, "apportioned_in"),
        Money(r, "apportioned_out"),
        Money(r, "net_cost"),
        Money(r, "variance"),
        Money(r, "share_pct"));

    private static ApportionmentRunSummary ParseRun(JsonElement r) => new(
        Text(r, "id"),
        Text(r, "period_key"),
        Text(r, "status"),
        Money(r, "total_pool"),
        Money(r, "total_allocated"),
        Money(r, "total_unallocated"));

    private static AnalyticalCenterNode ParseNode(JsonElement e) => new(
        Text(e, "id"),
        e.TryGetProperty("parent_id", out var parent) && parent.ValueKind == JsonValueKind.String ? parent.GetString() : null,
        Text(e, "code"),
        Text(e, "name"),
        Text(e, "center_type"),
        Text(e, "category"),
        e.TryGetProperty("manager_name", out var manager) && manager.ValueKind == JsonValueKind.String ? manager.GetString() : null,
        !e.TryGetProperty("is_active", out var active) || active.ValueKind != JsonValueKind.False,
        e.TryGetProperty("depth", out var depth) ? depth.GetInt32() : 0,
        ParseChildren(e));

    private static List<AnalyticalCenterNode> ParseChildren(JsonElement e) =>
        e.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array ? ParseArray(children, ParseNode) : [];

    private static string Text(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : throw new JsonException("Missing text property " + name + ".");

    private static decimal Money(JsonElement element, string name) =>
        !element.TryGetProperty(name, out var value)
            ? throw new JsonException("Missing amount property " + name + ".")
            : value switch
            {
                { ValueKind: JsonValueKind.Number } => value.GetDecimal(),
                { ValueKind: JsonValueKind.String } when decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => throw new JsonException("Invalid amount in " + name + "."),
            };
}
