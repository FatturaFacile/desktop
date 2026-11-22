// <copyright file="AnalyticalTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.UnitTests;

/// <summary>
/// Payloads of the analytical cost accounting endpoints shared by the Mod 018 desktop tests.
/// </summary>
internal static class AnalyticalTestData
{
    /// <summary>
    /// The center tree: a company root with an auxiliary IT child and a production child.
    /// </summary>
    public const string Centers =
        """[{"id":"root","parent_id":null,"code":"ROOT","name":"Company","center_type":"COST_CENTER","category":"STRUCTURAL","depth":0,"manager_name":"Rita Rossi","is_active":true,"children":[{"id":"it","parent_id":"root","code":"IT","name":"Information systems","center_type":"COST_CENTER","category":"AUXILIARY","depth":1,"manager_name":null,"is_active":true,"children":[]},{"id":"asm","parent_id":"root","code":"ASM","name":"Assembly","center_type":"COST_CENTER","category":"PRODUCTION","depth":1,"manager_name":null,"is_active":false,"children":[]}]}]""";

    /// <summary>
    /// The run history.
    /// </summary>
    public const string Runs =
        """[{"id":"r-1","period_key":"2027-03","status":"CONSOLIDATED","total_pool":"1000.0000","total_allocated":"1000.0000","total_unallocated":"0.0000"}]""";

    /// <summary>
    /// A consistent contribution margin report.
    /// </summary>
    public const string Margin =
        """{"statement":{"operating_revenues":"5000.00","direct_variable_costs":"400.00","contribution_margin_1":"4600.00","contribution_margin_1_pct":"92.00","direct_fixed_costs":"1000.00","contribution_margin_2":"3600.00","contribution_margin_2_pct":"72.00","indirect_costs":"0.00","apportioned_costs":"250.00","operating_result":"3350.00","operating_result_pct":"67.00","non_operating":"0.00"},"breakdown":[]}""";

    /// <summary>
    /// The direct-versus-apportioned comparison.
    /// </summary>
    public const string Comparison =
        """{"rows":[{"center_id":"it","code":"IT","name":"Information systems","center_type":"COST_CENTER","depth":1,"direct_cost":"1000.00","apportioned_in":"0.00","apportioned_out":"1000.00","net_cost":"0.00","variance":"-1000.00","variance_pct":"-100.00","share_pct":"0.00"}],"totals":{"direct_cost":"1000.00","apportioned_in":"0.00","apportioned_out":"1000.00","net_cost":"0.00"}}""";
}
