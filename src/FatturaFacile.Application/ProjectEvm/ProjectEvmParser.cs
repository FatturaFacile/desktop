// <copyright file="ProjectEvmParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// Reads the `data` objects of the profitability endpoints into read models. Amounts arrive as decimal strings. Missing or null
/// fields are tolerated, so a server that adds or drops a field does not break a cached snapshot.
/// </summary>
public static class ProjectEvmParser
{
    private const string DefaultCurrency = "EUR";

    /// <summary>
    /// Builds the snapshot of a project from the three stored payloads.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="payload">The payload.</param>
    /// <param name="syncedAtUtc">When it was pulled.</param>
    /// <returns>The snapshot.</returns>
    public static ProjectEvmSnapshot ParseSnapshot(string projectId, ProjectEvmPayload payload, DateTime syncedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(payload);

        using var breakdown = JsonDocument.Parse(payload.BreakdownJson);
        using var waterfall = JsonDocument.Parse(payload.WaterfallJson);
        using var scurve = JsonDocument.Parse(payload.ScurveJson);
        var root = breakdown.RootElement;
        var project = Object(root, "project");
        var reconciliation = Object(root, "reconciliation");

        List<ProjectEvmNode> nodes = [];
        foreach (var item in Items(root, "nodes"))
        {
            nodes.Add(ParseNode(item));
        }

        List<ProjectEvmWaterfallStep> steps = [];
        foreach (var item in Items(waterfall.RootElement, "steps"))
        {
            steps.Add(new ProjectEvmWaterfallStep(Text(item, "key") ?? string.Empty, Number(item, "amount") ?? 0, Number(item, "running_total") ?? 0, string.Equals(Text(item, "kind"), "total", StringComparison.Ordinal)));
        }

        List<ProjectEvmScurvePoint> points = [];
        foreach (var item in Items(scurve.RootElement, "points"))
        {
            if (Date(item, "date") is { } date)
            {
                points.Add(new ProjectEvmScurvePoint(date, Number(item, "pv") ?? 0, Number(item, "ev") ?? 0, Number(item, "ac") ?? 0));
            }
        }

        List<ProjectEvmEacPoint> forecast = [];
        foreach (var item in Items(scurve.RootElement, "forecast"))
        {
            if (Date(item, "date") is { } date)
            {
                forecast.Add(new ProjectEvmEacPoint(date, Number(item, "eac") ?? 0));
            }
        }

        return new ProjectEvmSnapshot(
            projectId,
            Text(root, "currency") ?? Text(waterfall.RootElement, "currency") ?? DefaultCurrency,
            Date(root, "snapshot_date"),
            Flag(project, "has_snapshot") ? ParseFigures(project) : null,
            reconciliation.ValueKind == JsonValueKind.Object ? ParseReconciliation(reconciliation) : null,
            nodes,
            steps,
            points,
            forecast,
            syncedAtUtc);
    }

    /// <summary>
    /// Reads the projects of the picker.
    /// </summary>
    /// <param name="json">The `data` array of /projects, or null.</param>
    /// <returns>The projects (none for a null payload).</returns>
    public static IReadOnlyList<ProjectEvmProject> ParseProjects(string? json)
    {
        List<ProjectEvmProject> projects = [];
        if (string.IsNullOrWhiteSpace(json))
        {
            return projects;
        }

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return projects;
        }

        for (var index = 0; index < document.RootElement.GetArrayLength(); index++)
        {
            var item = document.RootElement[index];
            projects.Add(new ProjectEvmProject(Text(item, "id") ?? string.Empty, Text(item, "project_number") ?? string.Empty, Text(item, "name") ?? string.Empty));
        }

        return projects;
    }

    private static ProjectEvmNode ParseNode(JsonElement element)
    {
        List<ProjectEvmNode> children = [];
        foreach (var child in Items(element, "children"))
        {
            children.Add(ParseNode(child));
        }

        return new ProjectEvmNode(
            Text(element, "wbs_node_id") ?? string.Empty,
            Text(element, "wbs_code") ?? string.Empty,
            Text(element, "title") ?? string.Empty,
            Flag(element, "has_snapshot") ? ParseFigures(element) : null,
            children);
    }

    private static ProjectEvmFigures ParseFigures(JsonElement e) =>
        new(
            Number(e, "bac") ?? 0,
            Number(e, "committed") ?? 0,
            Number(e, "actual") ?? 0,
            Number(e, "pv") ?? 0,
            Number(e, "ev") ?? 0,
            Number(e, "cv") ?? 0,
            Number(e, "sv") ?? 0,
            Number(e, "cpi"),
            Number(e, "spi"),
            Number(e, "eac") ?? 0,
            Number(e, "etc") ?? 0,
            Number(e, "vac") ?? 0,
            Number(e, "budget_absorption_percent"),
            Number(e, "recognized_revenue") ?? 0,
            Number(e, "gross_margin_amount") ?? 0,
            Number(e, "gross_margin_percent"),
            Number(e, "manual_etc_override"));

    private static ProjectEvmReconciliation ParseReconciliation(JsonElement e) =>
        new(
            Text(e, "standard"),
            Number(e, "managerial_actual_cost") ?? 0,
            Number(e, "statutory_cumulative_cost") ?? 0,
            Number(e, "cost_difference"),
            Number(e, "cost_center_overhead") ?? 0,
            Flag(e, "is_reconciled"),
            Text(e, "last_recognition_date") is not null);

    private static JsonElement Object(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object ? value : default;

    private static List<JsonElement> Items(JsonElement parent, string name)
    {
        List<JsonElement> items = [];
        if (parent.ValueKind != JsonValueKind.Object || !parent.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return items;
        }

        for (var index = 0; index < value.GetArrayLength(); index++)
        {
            if (value[index].ValueKind == JsonValueKind.Object)
            {
                items.Add(value[index]);
            }
        }

        return items;
    }

    private static string? Text(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool Flag(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;

    private static double? Number(JsonElement parent, string name) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : FromText(Text(parent, name));

    private static double? FromText(string? text) =>
        text is not null && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static DateTime? Date(JsonElement parent, string name) =>
        Text(parent, name) is { } text && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date) ? date : null;
}
