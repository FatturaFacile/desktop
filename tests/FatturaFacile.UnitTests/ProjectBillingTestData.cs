// <copyright file="ProjectBillingTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectBilling;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Payloads of the project billing endpoints shared by the billing tests.
/// </summary>
internal static class ProjectBillingTestData
{
    /// <summary>
    /// The project identifier of the payloads.
    /// </summary>
    public const string ProjectId = "11111111-2222-3333-4444-555555555555";

    /// <summary>
    /// The unbilled summary.
    /// </summary>
    public const string Summary = """{ "project_id": "11111111-2222-3333-4444-555555555555", "currency": "CHF", "billing_model": "hybrid", "contract_value": "100000.00", "billed_to_date": "27000.00", "draft_billing": "0.00", "unbilled_time_and_materials": "240.00", "unbilled_milestones": 1, "open_sal_certificates": 0, "open_sal_invoice_amount": "0.00", "retention": { "withheld": "1500.00", "released": "500.00", "balance": "1000.00", "release_rule": "on_acceptance", "release_date": null }, "latest_wip_run": { "period_end_date": "2027-03-31", "recognized_revenue_to_date": "20000.00", "billed_to_date": "50000.00", "wip_asset_or_deferred_revenue_diff": "-30000.00", "nature": "contract_liability" } }""";

    /// <summary>
    /// A billing contract by progress.
    /// </summary>
    public const string Contract = """{ "billing_model": "sal_progress", "billing_status": "active", "fiscal_jurisdiction": "IT", "currency_code": "CHF", "contract_total_value": "100000.0000", "advance_payment_percent": "10.0000", "advance_payment_amount": "0.0000", "advance_recouped_amount": "3000.0000", "retention_guarantee_percent": "5.0000", "cig": null }""";

    /// <summary>
    /// A live certificate and a cancelled one.
    /// </summary>
    public const string Certificates = """[ { "id": "s1", "sal_number": 1, "previous_completion_percent": "0.0000", "cumulative_completion_percent": "30.0000", "cumulative_gross_earned_value": "30000.0000", "current_period_gross_sal": "30000.0000", "advance_deduction_amount": "3000.0000", "guarantee_retention_withheld": "1500.0000", "invoice_amount": "27000.0000", "net_billable_amount": "25500.0000", "is_final": false, "status": "invoiced", "period_start_date": "2027-01-01", "period_end_date": "2027-01-31" }, { "id": "s2", "sal_number": 2, "previous_completion_percent": "30.0000", "cumulative_completion_percent": "60.0000", "cumulative_gross_earned_value": "60000.0000", "current_period_gross_sal": "30000.0000", "advance_deduction_amount": "3000.0000", "guarantee_retention_withheld": "1500.0000", "invoice_amount": "27000.0000", "net_billable_amount": "25500.0000", "is_final": false, "status": "cancelled" } ]""";

    /// <summary>
    /// The retention balance.
    /// </summary>
    public const string Retention = """{ "withheld": "1500.00", "released": "500.00", "balance": "1000.00", "release_rule": "on_acceptance" }""";

    /// <summary>
    /// A retainer nearly burned.
    /// </summary>
    public const string Retainer = """{ "balance": { "unit": "hours", "remaining_hours": "4.00", "remaining_amount": "0.00", "remaining_percent": "10.00", "consumed_hours": "36.00", "low_balance_threshold_percent": "25.00", "is_low_balance": true }, "entries": [] }""";

    /// <summary>
    /// The text of an absent optional payload.
    /// </summary>
    public const string Absent = "null";

    /// <summary>
    /// Builds the complete payload.
    /// </summary>
    /// <returns>The payload.</returns>
    public static ProjectBillingPayload Payload() => new(Summary, Contract, Certificates, Retention, Retainer);

    /// <summary>
    /// Builds the payload of a project that has no contract yet.
    /// </summary>
    /// <returns>The payload.</returns>
    public static ProjectBillingPayload PayloadWithoutContract() => new(Summary, Absent, "[]", Absent, Absent);
}
