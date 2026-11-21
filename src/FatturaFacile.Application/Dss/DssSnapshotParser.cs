// <copyright file="DssSnapshotParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using FatturaFacile.Domain.Dss;

namespace FatturaFacile.Application.Dss;

/// <summary>
/// Reads the `data` objects of the Executive DSS endpoints into read models. Missing or null fields are tolerated, so a server
/// that adds or drops a field does not break a cached snapshot.
/// </summary>
public static class DssSnapshotParser
{
    private const double Percent = 100.0;
    private const string DateFormat = "yyyy-MM-dd";

    private static readonly string[] ScorecardGroups = ["financial", "customer", "efficiency"];

    /// <summary>
    /// Parses the overview payload.
    /// </summary>
    /// <param name="json">The `data` object of /overview.</param>
    /// <returns>The overview.</returns>
    public static DssOverview ParseOverview(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var meta = Object(root, "meta");
        var period = Object(meta, "period");

        List<DssKpi> kpis = [];
        var scorecard = Object(root, "scorecard");
        foreach (var group in ScorecardGroups)
        {
            foreach (var item in Items(scorecard, group))
            {
                kpis.Add(ParseKpi(item, group));
            }
        }

        List<DssAlert> alerts = [];
        foreach (var item in Items(root, "alerts"))
        {
            alerts.Add(ParseAlert(item));
        }

        var capital = Object(root, "working_capital");
        DssWorkingCapital workingCapital = new(
            Number(capital, "dso_days") ?? 0,
            Number(capital, "dio_days") ?? 0,
            Number(capital, "dpo_days") ?? 0,
            Number(capital, "ccc_days") ?? 0);

        return new DssOverview(Text(meta, "currency") ?? "CHF", Text(period, "from") ?? string.Empty, Text(period, "to") ?? string.Empty, kpis, alerts, workingCapital);
    }

    /// <summary>
    /// Parses the profitability matrix payload.
    /// </summary>
    /// <param name="json">The `data` object of /profitability-matrix.</param>
    /// <returns>The matrix and Whale Curve.</returns>
    public static DssProfitability ParseProfitability(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        List<DssMatrixRow> rows = [];
        foreach (var row in Items(root, "rows"))
        {
            rows.Add(ParseRow(row));
        }

        List<DssWhalePoint> curve = [];
        foreach (var point in Items(root, "whale_curve"))
        {
            curve.Add(new DssWhalePoint(
                (int)(Number(point, "rank") ?? 0),
                Text(point, "label") ?? string.Empty,
                Number(point, "net_operating_profit") ?? 0,
                Number(point, "cumulative_profit_pct") ?? 0));
        }

        return new DssProfitability(Text(root, "dimension") ?? "customer", rows, curve, (int)(Number(root, "pareto_cutoff_rank") ?? 0));
    }

    /// <summary>
    /// Parses the variance bridge payload.
    /// </summary>
    /// <param name="json">The `data` object of /variance-bridge.</param>
    /// <returns>The bridges.</returns>
    public static DssVarianceBridge ParseVarianceBridge(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new DssVarianceBridge(ParseSteps(root, "ebitda_bridge"), ParseSteps(root, "revenue_bridge"), Text(root, "volume_basis") ?? "invoice_items");
    }

    /// <summary>
    /// Parses the legal entities payload.
    /// </summary>
    /// <param name="json">The `data` array of /legal-entities, or null.</param>
    /// <returns>The legal entities (none for a null payload).</returns>
    public static IReadOnlyList<DssLegalEntity> ParseLegalEntities(string? json)
    {
        List<DssLegalEntity> entities = [];
        if (string.IsNullOrWhiteSpace(json))
        {
            return entities;
        }

        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return entities;
        }

        for (var index = 0; index < document.RootElement.GetArrayLength(); index++)
        {
            var item = document.RootElement[index];
            entities.Add(new DssLegalEntity(Text(item, "id") ?? string.Empty, Text(item, "name") ?? string.Empty, Text(item, "currency") ?? string.Empty));
        }

        return entities;
    }

    /// <summary>
    /// Builds the actuals of the offline simulator. The server inputs are used when they were fetched; otherwise the actuals are
    /// derived from the scorecard (revenue, gross margin, EBITDA, DSO, net cash flow and the period), without FX exposure and FTE cost.
    /// </summary>
    /// <param name="overview">The overview.</param>
    /// <param name="simulationInputsJson">The `data` object of /simulate, or null.</param>
    /// <returns>The baseline.</returns>
    public static WhatIfBaseline ParseBaseline(DssOverview overview, string? simulationInputsJson)
    {
        ArgumentNullException.ThrowIfNull(overview);

        return string.IsNullOrWhiteSpace(simulationInputsJson) ? Derive(overview) : FromInputs(overview, simulationInputsJson);
    }

    private static WhatIfBaseline FromInputs(DssOverview overview, string simulationInputsJson)
    {
        using var document = JsonDocument.Parse(simulationInputsJson);
        var inputs = Object(document.RootElement, "inputs");
        var revenue = Number(inputs, "revenue");

        return revenue is null
            ? Derive(overview)
            : new WhatIfBaseline(
                revenue.Value,
                Number(inputs, "cogs") ?? 0,
                Number(inputs, "opex") ?? 0,
                Number(inputs, "dso_days") ?? 0,
                Number(inputs, "net_cash_flow") ?? 0,
                (int)(Number(inputs, "period_days") ?? PeriodDays(overview)),
                Number(inputs, "fte_annual_cost") ?? 0,
                Number(inputs, "revenue_fx_share") ?? 0,
                Number(inputs, "cost_fx_share") ?? 0);
    }

    private static WhatIfBaseline Derive(DssOverview overview)
    {
        var revenue = ValueOf(overview, "revenue");
        var cogs = revenue * (1 - (ValueOf(overview, "gross_margin_pct") / Percent));
        var opex = revenue - cogs - ValueOf(overview, "ebitda");

        return new WhatIfBaseline(revenue, cogs, opex, overview.WorkingCapital.DsoDays, ValueOf(overview, "net_cash_flow"), PeriodDays(overview));
    }

    private static double ValueOf(DssOverview overview, string key)
    {
        foreach (var kpi in overview.Kpis)
        {
            if (string.Equals(kpi.Key, key, StringComparison.Ordinal))
            {
                return kpi.Value ?? 0;
            }
        }

        return 0;
    }

    private static int PeriodDays(DssOverview overview)
    {
        var hasFrom = DateTime.TryParseExact(overview.PeriodFrom, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var from);
        var hasTo = DateTime.TryParseExact(overview.PeriodTo, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var to);

        return hasFrom && hasTo ? Math.Max((to - from).Days + 1, 1) : 1;
    }

    private static DssKpi ParseKpi(JsonElement item, string group)
    {
        List<double> sparkline = [];
        foreach (var point in Items(item, "sparkline"))
        {
            if (point.ValueKind == JsonValueKind.Number)
            {
                sparkline.Add(point.GetDouble());
            }
        }

        return new DssKpi(
            Text(item, "key") ?? string.Empty,
            group,
            Text(item, "unit") ?? "ratio",
            Flag(item, "available") != false,
            Number(item, "value"),
            Number(item, "comparison_value"),
            Number(item, "delta"),
            Number(item, "delta_pct"),
            Flag(item, "favorable"),
            sparkline,
            Number(item, "target"));
    }

    private static DssAlert ParseAlert(JsonElement item)
    {
        var context = Object(item, "context");

        return new DssAlert(
            Text(item, "code") ?? string.Empty,
            Text(item, "severity") ?? "INFO",
            Text(item, "metric") ?? string.Empty,
            Number(item, "value") ?? 0,
            Number(item, "threshold") ?? 0,
            Text(context, "label"));
    }

    private static DssMatrixRow ParseRow(JsonElement row) => new(
        Text(row, "key") ?? string.Empty,
        Text(row, "label") ?? string.Empty,
        (int)(Number(row, "rank") ?? 0),
        Number(row, "net_revenue") ?? 0,
        Number(row, "gross_margin") ?? 0,
        Number(row, "gross_margin_pct") ?? 0,
        Number(row, "contribution_margin_1") ?? 0,
        Number(row, "contribution_margin_2") ?? 0,
        Number(row, "ebitda") ?? 0,
        Number(row, "payment_delay_cost") ?? 0,
        Number(row, "net_operating_profit") ?? 0,
        Number(row, "net_margin_pct") ?? 0,
        Flag(row, "in_pareto_core") == true,
        Flag(row, "is_value_destroying") == true);

    private static List<DssBridgeStep> ParseSteps(JsonElement root, string name)
    {
        List<DssBridgeStep> steps = [];
        foreach (var step in Items(root, name))
        {
            steps.Add(new DssBridgeStep(
                Text(step, "key") ?? string.Empty,
                string.Equals(Text(step, "type"), "total", StringComparison.Ordinal),
                Number(step, "value") ?? 0,
                Number(step, "cumulative") ?? 0));
        }

        return steps;
    }

    private static JsonElement Object(JsonElement parent, string name)
    {
        return parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object
            ? value
            : default;
    }

    private static List<JsonElement> Items(JsonElement parent, string name)
    {
        List<JsonElement> items = [];
        if (parent.ValueKind != JsonValueKind.Object || !parent.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return items;
        }

        for (var index = 0; index < value.GetArrayLength(); index++)
        {
            items.Add(value[index]);
        }

        return items;
    }

    private static string? Text(JsonElement parent, string name)
    {
        return parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static double? Number(JsonElement parent, string name)
    {
        return parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;
    }

    private static bool? Flag(JsonElement parent, string name)
    {
        JsonElement value = default;
        var found = parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out value);
        bool? flag = null;

        if (found && value.ValueKind == JsonValueKind.True)
        {
            flag = true;
        }
        else if (found && value.ValueKind == JsonValueKind.False)
        {
            flag = false;
        }

        return flag;
    }
}
