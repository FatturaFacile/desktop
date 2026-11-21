// <copyright file="CockpitMetricRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A flattened metric of the cached cockpit snapshot.
/// </summary>
/// <param name="Metric">The dotted metric path.</param>
/// <param name="Value">The value as text.</param>
internal sealed record CockpitMetricRow(string Metric, string Value)
{
    private const int MaxDepth = 3;

    /// <summary>
    /// Flattens the cockpit JSON into scalar rows (arrays show their length), so the cached snapshot is readable offline.
    /// </summary>
    /// <param name="json">The cockpit JSON.</param>
    /// <returns>The rows, empty when the JSON is invalid.</returns>
    public static IReadOnlyList<CockpitMetricRow> Flatten(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            List<CockpitMetricRow> rows = [];
            Walk(document.RootElement, string.Empty, depth: 0, rows);
            return rows;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? Describe(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Array => element.GetArrayLength().ToString(CultureInfo.CurrentCulture),
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.ToString(),
        JsonValueKind.Undefined or JsonValueKind.Object or JsonValueKind.Null => null,
        _ => null,
    };

    private static void Walk(JsonElement element, string path, int depth, List<CockpitMetricRow> rows)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (depth >= MaxDepth)
            {
                return;
            }

            var members = element.Deserialize<Dictionary<string, JsonElement>>();
            if (members is null)
            {
                return;
            }

            foreach (var (name, value) in members)
            {
                Walk(value, path.Length == 0 ? name : path + "." + name, depth + 1, rows);
            }

            return;
        }

        if (Describe(element) is { } text)
        {
            rows.Add(new CockpitMetricRow(path, text));
        }
    }
}
