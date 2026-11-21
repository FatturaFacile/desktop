// <copyright file="DssTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Sample answers of the web-api Executive DSS endpoints (the `data` objects) shared by the DSS tests.
/// </summary>
internal static class DssTestData
{
    /// <summary>
    /// The overview answer: a scorecard of eight indicators, two alerts and the working capital.
    /// </summary>
    public const string Overview = """{"meta":{"currency":"CHF","period":{"preset":"YTD","from":"2026-01-01","to":"2026-06-30"}},"scorecard":{"financial":[{"key":"revenue","unit":"currency","available":true,"value":1000,"comparison_value":500,"delta":500,"delta_pct":100,"favorable":true,"sparkline":[100,200,300],"target":null},{"key":"gross_margin_pct","unit":"percent","available":true,"value":60,"comparison_value":65,"delta":-5,"delta_pct":-7.7,"favorable":false,"sparkline":[],"target":null},{"key":"ebitda","unit":"currency","available":true,"value":400,"comparison_value":null,"delta":null,"delta_pct":null,"favorable":null,"sparkline":[],"target":null},{"key":"net_cash_flow","unit":"currency","available":true,"value":500,"comparison_value":null,"delta":null,"delta_pct":null,"favorable":null,"sparkline":[],"target":null},{"key":"cash_conversion_cycle","unit":"days","available":true,"value":-126.7,"comparison_value":null,"delta":null,"delta_pct":null,"favorable":null,"sparkline":[],"target":30}],"customer":[{"key":"average_dso","unit":"days","available":true,"value":54.3,"comparison_value":null,"delta":4.3,"delta_pct":null,"favorable":false,"sparkline":[],"target":null},{"key":"customer_concentration_index","unit":"percent","available":true,"value":90,"comparison_value":null,"delta":null,"delta_pct":null,"favorable":null,"sparkline":[],"target":null}],"efficiency":[{"key":"on_time_delivery_pct","unit":"percent","available":false,"value":null,"comparison_value":null,"delta":null,"delta_pct":null,"favorable":null,"sparkline":[],"target":null}]},"alerts":[{"code":"customer_concentration","severity":"CRITICAL","metric":"customer_share_pct","value":90,"threshold":25,"context":{"counterparty_id":"c1","label":"Alpenbau AG"}},{"code":"dso_spike","severity":"WARNING","metric":"dso_days","value":9,"threshold":7,"context":{}}],"working_capital":{"dso_days":54.3,"dio_days":0,"dpo_days":181,"ccc_days":-126.7}}""";

    /// <summary>
    /// The profitability matrix answer: two customers, one of them loss making, and the Whale Curve.
    /// </summary>
    public const string Profitability = """{"dimension":"customer","rows":[{"key":"c1","label":"Alpenbau AG","rank":1,"net_revenue":900,"gross_margin":540,"gross_margin_pct":60,"contribution_margin_1":540,"contribution_margin_2":540,"ebitda":360,"payment_delay_cost":1.3,"net_operating_profit":358.7,"net_margin_pct":39.9,"in_pareto_core":true,"is_value_destroying":false},{"key":"c2","label":"Verlust GmbH","rank":2,"net_revenue":100,"gross_margin":10,"gross_margin_pct":10,"contribution_margin_1":10,"contribution_margin_2":10,"ebitda":-40,"payment_delay_cost":0,"net_operating_profit":-40,"net_margin_pct":-40,"in_pareto_core":false,"is_value_destroying":true}],"whale_curve":[{"rank":1,"label":"Alpenbau AG","net_operating_profit":358.7,"cumulative_profit_pct":112.6},{"rank":2,"label":"Verlust GmbH","net_operating_profit":-40,"cumulative_profit_pct":100}],"pareto_cutoff_rank":1}""";

    /// <summary>
    /// The variance bridge answer: seven EBITDA steps and five revenue steps.
    /// </summary>
    public const string VarianceBridge = """{"volume_basis":"invoice_items","ebitda_bridge":[{"key":"baseline","type":"total","value":200,"cumulative":200},{"key":"volume","type":"delta","value":80,"cumulative":280},{"key":"price","type":"delta","value":120,"cumulative":400},{"key":"cogs","type":"delta","value":-60,"cumulative":340},{"key":"opex","type":"delta","value":-20,"cumulative":320},{"key":"fx","type":"delta","value":0,"cumulative":320},{"key":"actual","type":"total","value":320,"cumulative":320}],"revenue_bridge":[{"key":"baseline","type":"total","value":1000,"cumulative":1000},{"key":"volume","type":"delta","value":200,"cumulative":1200},{"key":"price","type":"delta","value":120,"cumulative":1320},{"key":"fx","type":"delta","value":0,"cumulative":1320},{"key":"actual","type":"total","value":1320,"cumulative":1320}]}""";

    /// <summary>
    /// The what-if inputs of the server (the `data` object of a neutral simulation).
    /// </summary>
    public const string SimulationInputs = """{"inputs":{"revenue":1000,"cogs":400,"opex":200,"dso_days":54.3,"net_cash_flow":500,"period_days":181,"fte_annual_cost":50,"revenue_fx_share":0.5,"cost_fx_share":0.2}}""";

    /// <summary>
    /// The legal entities answer.
    /// </summary>
    public const string LegalEntities = """[{"id":"ent-1","name":"Alpha AG","currency":"CHF"},{"id":"ent-2","name":"Beta Srl","currency":"EUR"}]""";

    /// <summary>
    /// Builds a payload of the sample answers.
    /// </summary>
    /// <param name="inputs">The what-if inputs, or null.</param>
    /// <param name="entities">The legal entities, or null.</param>
    /// <returns>The payload.</returns>
    public static DssPayload Payload(string? inputs = SimulationInputs, string? entities = LegalEntities) =>
        new(Overview, Profitability, VarianceBridge, inputs, entities);

    /// <summary>
    /// Asserts that two sequences of strings are equal, comparing ordinally.
    /// </summary>
    /// <param name="expected">The expected strings.</param>
    /// <param name="actual">The actual strings.</param>
    public static void SameStrings(IEnumerable<string> expected, IEnumerable<string> actual) =>
        Assert.Equal(expected, actual, StringComparer.Ordinal);
}
