// <copyright file="ProjectEvmTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectEvm;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Payloads of the project profitability endpoints, as the web-api sends them (amounts are decimal strings).
/// </summary>
internal static class ProjectEvmTestData
{
    /// <summary>The id of the sample project.</summary>
    public const string ProjectId = "11111111-1111-1111-1111-111111111111";

    /// <summary>The WBS breakdown payload.</summary>
    public static readonly string Breakdown = Json(
        "{'project_id':'11111111-1111-1111-1111-111111111111','currency':'CHF','snapshot_date':'2027-03-01'," +
        "'project':{'has_snapshot':true,'bac':'100000.00','committed':'1000.00','actual':'30000.00','total_exposure':'31000.00'," +
        "'pv':'50000.00','ev':'25000.00','cv':'-5000.00','sv':'-25000.00','cpi':'0.8333','spi':'0.5000'," +
        "'eac':'120000.00','etc':'90000.00','vac':'-20000.00','budget_absorption_percent':'85.00'," +
        "'recognized_revenue':'50000.00','gross_margin_amount':'20000.00','gross_margin_percent':'40.00','manual_etc_override':null}," +
        "'reconciliation':{'standard':'Swiss GAAP FER 22','managerial_actual_cost':'30000.0000','statutory_cumulative_cost':'29000.0000'," +
        "'cost_difference':'1000.0000','cost_center_overhead':'500.0000','last_recognition_date':'2027-02-28','is_reconciled':false}," +
        "'nodes':[{'wbs_node_id':'n1','wbs_code':'1','title':'Phase one','has_snapshot':true,'bac':'100000.00','committed':'0.00'," +
        "'actual':'30000.00','pv':'50000.00','ev':'25000.00','cv':'-5000.00','sv':'-25000.00','cpi':'1.0500','spi':'0.9500'," +
        "'eac':'95000.00','etc':'65000.00','vac':'5000.00','budget_absorption_percent':'30.00','recognized_revenue':'0.00'," +
        "'gross_margin_amount':'0.00','gross_margin_percent':null,'manual_etc_override':'65000.00'," +
        "'children':[{'wbs_node_id':'n11','wbs_code':'1.1','title':'Not started','has_snapshot':false,'children':[]}]}]}");

    /// <summary>The profitability waterfall payload.</summary>
    public static readonly string Waterfall = Json(
        "{'currency':'CHF','steps':[" +
        "{'key':'revenue','amount':'50000.00','running_total':'50000.00','kind':'total'}," +
        "{'key':'TimesheetLabor','amount':'-30000.00','running_total':'20000.00','kind':'delta'}," +
        "{'key':'gross_margin','amount':'20000.00','running_total':'20000.00','kind':'total'}]}");

    /// <summary>The S-curve payload.</summary>
    public static readonly string Scurve = Json(
        "{'points':[{'date':'2027-01-31','pv':'100.00','ev':'80.00','ac':'90.00'}," +
        "{'date':'2027-02-28','pv':'200.00','ev':'150.00','ac':'170.00'}]," +
        "'forecast':[{'date':'2027-02-28','eac':'170.00'},{'date':'2027-06-30','eac':'600.00'}]}");

    /// <summary>The project picker payload.</summary>
    public static readonly string Projects = Json(
        "[{'id':'11111111-1111-1111-1111-111111111111','project_number':'PRJ-1','name':'Microgrid'}," +
        "{'id':'22222222-2222-2222-2222-222222222222','project_number':'','name':'Retrofit'}]");

    /// <summary>Builds the payload bundle of the sample project.</summary>
    /// <returns>The bundle.</returns>
    public static ProjectEvmPayload Payload() => new(Breakdown, Waterfall, Scurve);

    private static string Json(string quoted) => quoted.Replace('\'', '"');
}
