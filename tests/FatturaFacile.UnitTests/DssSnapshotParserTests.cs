// <copyright file="DssSnapshotParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.Dss;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the parsing of the web-api Executive DSS answers into read models.
/// </summary>
public sealed class DssSnapshotParserTests
{
    /// <summary>
    /// The overview keeps the groups, the values, the trend and the missing comparison of each indicator.
    /// </summary>
    [Fact]
    public void ParseOverview_ReadsScorecardAlertsAndWorkingCapital()
    {
        var overview = DssSnapshotParser.ParseOverview(DssTestData.Overview);

        Assert.Equal("CHF", overview.Currency);
        Assert.Equal("2026-01-01", overview.PeriodFrom);
        Assert.Equal(8, overview.Kpis.Count);
        Assert.Equal(5, overview.Kpis.Count(kpi => string.Equals(kpi.Group, "financial", StringComparison.Ordinal)));
        Assert.Equal(2, overview.Kpis.Count(kpi => string.Equals(kpi.Group, "customer", StringComparison.Ordinal)));

        var revenue = overview.Kpis.Single(kpi => string.Equals(kpi.Key, "revenue", StringComparison.Ordinal));
        Assert.Equal(1000, revenue.Value);
        Assert.Equal(100, revenue.DeltaPct);
        Assert.True(revenue.Favorable);
        Assert.Equal([100d, 200d, 300d], revenue.Sparkline);

        var ebitda = overview.Kpis.Single(kpi => string.Equals(kpi.Key, "ebitda", StringComparison.Ordinal));
        Assert.Null(ebitda.ComparisonValue);
        Assert.Null(ebitda.Favorable);

        Assert.Equal(30, overview.Kpis.Single(kpi => string.Equals(kpi.Key, "cash_conversion_cycle", StringComparison.Ordinal)).Target);
        Assert.Equal(54.3, overview.WorkingCapital.DsoDays);
        Assert.Equal(181, overview.WorkingCapital.DpoDays);
    }

    /// <summary>
    /// An indicator without a data source is read as unavailable with no value.
    /// </summary>
    [Fact]
    public void ParseOverview_UnavailableIndicator_HasNoValue()
    {
        var kpi = DssSnapshotParser.ParseOverview(DssTestData.Overview).Kpis.Single(item => string.Equals(item.Key, "on_time_delivery_pct", StringComparison.Ordinal));

        Assert.False(kpi.Available);
        Assert.Null(kpi.Value);
    }

    /// <summary>
    /// Alerts keep their severity and the customer they name.
    /// </summary>
    [Fact]
    public void ParseOverview_Alerts_KeepSeverityAndLabel()
    {
        var alerts = DssSnapshotParser.ParseOverview(DssTestData.Overview).Alerts;

        Assert.Equal(2, alerts.Count);
        Assert.Equal("CRITICAL", alerts[0].Severity);
        Assert.Equal("Alpenbau AG", alerts[0].Label);
        Assert.Null(alerts[1].Label);
        Assert.Equal(7, alerts[1].Threshold);
    }

    /// <summary>
    /// A server that drops fields does not break the parse.
    /// </summary>
    [Fact]
    public void ParseOverview_MissingSections_YieldsEmptyModel()
    {
        var overview = DssSnapshotParser.ParseOverview("{}");

        Assert.Equal("CHF", overview.Currency);
        Assert.Empty(overview.Kpis);
        Assert.Empty(overview.Alerts);
        Assert.Equal(0, overview.WorkingCapital.CccDays);
    }

    /// <summary>
    /// A payload that is not JSON is rejected, so it can never replace a good cache.
    /// </summary>
    [Fact]
    public void ParseOverview_InvalidJson_Throws() =>
        Assert.ThrowsAny<JsonException>(() => DssSnapshotParser.ParseOverview("not json"));

    /// <summary>
    /// The matrix and the Whale Curve keep the ranking, the flags and the Pareto cut-off.
    /// </summary>
    [Fact]
    public void ParseProfitability_ReadsRowsWhaleCurveAndCutoff()
    {
        var matrix = DssSnapshotParser.ParseProfitability(DssTestData.Profitability);

        Assert.Equal("customer", matrix.Dimension);
        Assert.Equal(2, matrix.Rows.Count);
        Assert.Equal("Alpenbau AG", matrix.Rows[0].Label);
        Assert.True(matrix.Rows[0].InParetoCore);
        Assert.True(matrix.Rows[1].IsValueDestroying);
        Assert.Equal(-40, matrix.Rows[1].NetOperatingProfit);
        Assert.Equal(2, matrix.WhaleCurve.Count);
        Assert.Equal(112.6, matrix.WhaleCurve[0].CumulativeProfitPct);
        Assert.Equal(1, matrix.ParetoCutoffRank);
    }

    /// <summary>
    /// The bridges keep totals apart from changes, in order.
    /// </summary>
    [Fact]
    public void ParseVarianceBridge_ReadsBothBridges()
    {
        var bridge = DssSnapshotParser.ParseVarianceBridge(DssTestData.VarianceBridge);

        Assert.Equal("invoice_items", bridge.VolumeBasis);
        DssTestData.SameStrings(["baseline", "volume", "price", "cogs", "opex", "fx", "actual"], bridge.EbitdaSteps.Select(step => step.Key));
        Assert.True(bridge.EbitdaSteps[0].IsTotal);
        Assert.False(bridge.EbitdaSteps[1].IsTotal);
        Assert.Equal(-60, bridge.EbitdaSteps[3].Value);
        Assert.Equal(5, bridge.RevenueSteps.Count);
        Assert.Equal(1320, bridge.RevenueSteps[^1].Cumulative);
    }

    /// <summary>
    /// The legal entities are optional.
    /// </summary>
    [Fact]
    public void ParseLegalEntities_ReadsListAndToleratesNothing()
    {
        var entities = DssSnapshotParser.ParseLegalEntities(DssTestData.LegalEntities);

        DssTestData.SameStrings(["ent-1", "ent-2"], entities.Select(entity => entity.Id));
        Assert.Equal("EUR", entities[1].Currency);
        Assert.Empty(DssSnapshotParser.ParseLegalEntities(json: null));
        Assert.Empty(DssSnapshotParser.ParseLegalEntities("  "));
        Assert.Empty(DssSnapshotParser.ParseLegalEntities("{}"));
    }

    /// <summary>
    /// With the server inputs the offline simulator starts from exactly the figures of the server, FX exposure and FTE cost included.
    /// </summary>
    [Fact]
    public void ParseBaseline_WithServerInputs_UsesThem()
    {
        var overview = DssSnapshotParser.ParseOverview(DssTestData.Overview);

        var baseline = DssSnapshotParser.ParseBaseline(overview, DssTestData.SimulationInputs);

        Assert.Equal(1000, baseline.Revenue);
        Assert.Equal(400, baseline.Cogs);
        Assert.Equal(200, baseline.Opex);
        Assert.Equal(181, baseline.PeriodDays);
        Assert.Equal(50, baseline.FteAnnualCost);
        Assert.Equal(0.5, baseline.RevenueFxShare);
        Assert.Equal(0.2, baseline.CostFxShare);
    }

    /// <summary>
    /// Without the inputs the actuals are derived from the scorecard: COGS from the gross margin, opex from the EBITDA.
    /// </summary>
    [Fact]
    public void ParseBaseline_WithoutInputs_DerivesFromTheScorecard()
    {
        var overview = DssSnapshotParser.ParseOverview(DssTestData.Overview);

        var baseline = DssSnapshotParser.ParseBaseline(overview, simulationInputsJson: null);

        Assert.Equal(1000, baseline.Revenue);
        Assert.Equal(400, baseline.Cogs, 6);
        Assert.Equal(200, baseline.Opex, 6);
        Assert.Equal(54.3, baseline.DsoDays);
        Assert.Equal(500, baseline.NetCashFlow);

        // 2026-01-01 to 2026-06-30 inclusive.
        Assert.Equal(181, baseline.PeriodDays);
        Assert.Equal(0, baseline.RevenueFxShare);
    }

    /// <summary>
    /// Inputs without a revenue (a server that refused the simulation) fall back to the derived actuals.
    /// </summary>
    [Fact]
    public void ParseBaseline_InputsWithoutRevenue_FallBackToDerived()
    {
        var overview = DssSnapshotParser.ParseOverview(DssTestData.Overview);

        var baseline = DssSnapshotParser.ParseBaseline(overview, """{ "inputs": {} }""");

        Assert.Equal(400, baseline.Cogs, 6);
    }
}
