// <copyright file="SupplierRebatesTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Payloads shaped like the answers of the supplier rebate endpoints of the web-api, and the read models they map to.
/// </summary>
internal static class SupplierRebatesTestData
{
    /// <summary>
    /// The agreement id.
    /// </summary>
    internal const string AgreementId = "6f1d1c0e-0000-4000-8000-000000000001";

    /// <summary>
    /// The supplier id.
    /// </summary>
    internal const string SupplierId = "6f1d1c0e-0000-4000-8000-0000000000c1";

    /// <summary>
    /// The contract id.
    /// </summary>
    internal const string ContractId = "6f1d1c0e-0000-4000-8000-0000000000b1";

    /// <summary>
    /// The agreement json.
    /// </summary>
    internal const string AgreementJson = """
        {"id":"6f1d1c0e-0000-4000-8000-000000000001","code":"PFA-2027","name":"Annual rebate","direction":"purchase","contact_id":"6f1d1c0e-0000-4000-8000-0000000000c1","contact_name":"Rossi SA","tier_mode":"retroactive","starts_on":"2027-01-01","ends_on":"2027-12-31","currency":"CHF","status":"active","notes":null,"rebate_type":"tiered_volume","supplier_contract_id":null,"fixed_bonus_target":null,"fixed_bonus_amount":null,"accrual_frequency":"quarterly","accrual_account_id":"acc-1","income_account_id":"acc-2","version":3,"open_rebate":"250.50","tiers":[{"id":"t1","threshold_amount":"10000.00","rate_percent":"1.00","flat_amount":null},{"id":"t2","threshold_amount":"50000.00","rate_percent":"2.00","flat_amount":"100.00"}],"contributions":[{"id":"k1","kind":"co_marketing","description":"Flyer","amount":"500.00","due_on":"2027-06-30","status":"planned"}]}
        """;

    /// <summary>
    /// The progress json.
    /// </summary>
    internal const string ProgressJson = """
        {"agreement_id":"6f1d1c0e-0000-4000-8000-000000000001","rebate_type":"tiered_volume","tier_mode":"retroactive","currency":"CHF","as_of":"2027-03-31","qualifying_volume":"20000.00","earned_rebate":"200.00","accrued_rebate":"150.00","settled_rebate":"0.00","tier":{"current_tier":{"threshold_amount":"10000.00","rate_percent":"1.00","flat_amount":null},"next_tier":{"threshold_amount":"50000.00","rate_percent":"2.00","flat_amount":"100.00"},"accrued_rebate":"200.00","distance_to_next":"30000.00","progress_percent":"25.00","projected_rebate_at_next":"1100.00","projected_gain":"900.00"},"bonus":null,"contributions":{"planned":"500.00","accrued":"0.00","settled":"0.00"},"tiers":[{"threshold_amount":"10000.00","max_threshold":"50000.00","rate_percent":"1.00","flat_amount":null},{"threshold_amount":"50000.00","max_threshold":null,"rate_percent":"2.00","flat_amount":"100.00"}]}
        """;

    /// <summary>
    /// The bonus progress json.
    /// </summary>
    internal const string BonusProgressJson = """
        {"agreement_id":"a2","rebate_type":"fixed_bonus","tier_mode":"retroactive","currency":"CHF","as_of":"2027-03-31","qualifying_volume":"25000.00","earned_rebate":"0.00","accrued_rebate":"0.00","settled_rebate":"0.00","tier":null,"bonus":{"target":"30000.00","bonus":"750.00","reached":false,"distance_to_target":"5000.00","progress_percent":"83.33"},"contributions":{"planned":"0.00","accrued":"0.00","settled":"0.00"},"tiers":[]}
        """;

    /// <summary>
    /// The runs json.
    /// </summary>
    internal const string RunsJson = """
        [{"id":"r1","agreement_id":"6f1d1c0e-0000-4000-8000-000000000001","period_start":"2027-01-01","period_end":"2027-03-31","status":"posted","base_amount":"20000.00","cumulative_rebate":"200.00","delta_amount":"200.00","journal_entry_id":"j1","reversal_journal_entry_id":null,"reversed_at":null,"reversal_reason":null,"lines":[{"passive_invoice_id":"p1","invoice_number":"F-1","document_type":"TD01","invoice_date":"2027-02-01","net_amount":"21000.00"},{"passive_invoice_id":"p2","invoice_number":"NC-1","document_type":"TD04","invoice_date":"2027-03-01","net_amount":"-1000.00"}]}]
        """;

    /// <summary>
    /// The proposals json.
    /// </summary>
    internal const string ProposalsJson = """
        [{"id":"s1","agreement_id":"6f1d1c0e-0000-4000-8000-000000000001","amount":"200.00","gross_amount":"216.20","tax_rate":"8.10","tax_amount":"16.20","currency":"CHF","status":"pending_approval","execution_mode":"ap_set_off","passive_invoice_id":"p9","set_off_amount":null,"version":2,"lines":[{"id":"l1","line_type":"invoice","reference":"F-1","reference_date":"2027-02-01","net_amount":"21000.00","rebate_amount":"210.00"},{"id":"l2","line_type":"credit_note","reference":"NC-1","reference_date":"2027-03-01","net_amount":"-1000.00","rebate_amount":"-10.00"}]}]
        """;

    /// <summary>
    /// The balance json.
    /// </summary>
    internal const string BalanceJson = """
        {"contract_id":"6f1d1c0e-0000-4000-8000-0000000000b1","currency":"CHF","status":"active","ceiling_amount":"100000.00","minimum_spend_amount":"60000.00","ordered_amount":"85000.00","received_amount":"40000.00","invoiced_amount":"30000.00","remaining_ceiling":"15000.00","ceiling_used_percent":"85.00","minimum_shortfall":"0.00","minimum_reached_percent":"141.67","alert_threshold_percent":"80.00","lines":[{"line_id":"l1","description":"Widgets","unit_price":"12.50","committed_quantity":"10000.0000","released_quantity":"6800.0000","remaining_quantity":"3200.0000","ordered_amount":"85000.00"}],"alerts":[{"code":"ceiling_near","severity":"warning"}]}
        """;

    /// <summary>
    /// The contracts json.
    /// </summary>
    internal const string ContractsJson = """
        [{"id":"6f1d1c0e-0000-4000-8000-0000000000b1","contract_number":"RQ-2027-01","title":"Widgets","supplier_id":"6f1d1c0e-0000-4000-8000-0000000000c1","status":"active"}]
        """;

    /// <summary>
    /// The passive invoices json.
    /// </summary>
    internal const string PassiveInvoicesJson = """
        {"success":true,"message":"Passive invoices retrieved","data":{"data":[{"id":"p1","supplier_id":"6f1d1c0e-0000-4000-8000-0000000000c1","invoice_number":"F-1","date":"2027-02-01T00:00:00Z","total_amount":1000,"total_paid":"400.00","document_type":"TD01","currency":"CHF"},{"id":"p2","supplier_id":"6f1d1c0e-0000-4000-8000-0000000000c1","invoice_number":"NC-1","date":"2027-03-01","total_amount":"100.00","total_paid":"0","document_type":"TD04"},{"id":"p3","supplier_id":"other","invoice_number":"F-9","date":"2027-01-01","total_amount":"500.00","total_paid":"0","document_type":"TD01"},{"id":"p4","supplier_id":"6f1d1c0e-0000-4000-8000-0000000000c1","invoice_number":"F-0","date":"2027-01-01","total_amount":"300.00","total_paid":"300.00","document_type":"TD01"}]}}
        """;

    /// <summary>
    /// Gets the scale of the sample agreement.
    /// </summary>
    internal static IReadOnlyList<RebateTier> Scale { get; } = [new RebateTier(10000m, 1m), new RebateTier(50000m, 2m, 100m)];

    /// <summary>
    /// Builds an agreement.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="type">The rebate type.</param>
    /// <returns>The agreement.</returns>
    internal static RebateAgreement Agreement(string status = RebateCodes.StatusActive, string type = RebateCodes.TypeTieredVolume) =>
        new(
            AgreementId,
            "PFA-2027",
            "Annual rebate",
            RebateCodes.DirectionPurchase,
            SupplierId,
            "Rossi SA",
            RebateTierMode.Retroactive,
            new DateOnly(2027, 1, 1),
            new DateOnly(2027, 12, 31),
            "CHF",
            status,
            Notes: null,
            type,
            SupplierContractId: null,
            FixedBonusTarget: 30000m,
            FixedBonusAmount: 750m,
            RebateCodes.FrequencyQuarterly,
            AccrualAccountId: "acc-1",
            IncomeAccountId: "acc-2",
            Version: 3,
            OpenRebate: 200m,
            Scale,
            [new RebateContribution("k1", RebateCodes.KindListing, "Listing fee", 500m, new DateOnly(2027, 6, 30), "planned")]);

    /// <summary>
    /// Builds the progress of the sample agreement at 20 000 spend, the figures the server answers.
    /// </summary>
    /// <returns>The progress.</returns>
    internal static RebateProgress Progress() => SupplierRebateParser.ParseProgress(ProgressJson);

    /// <summary>
    /// Builds a posted accrual run.
    /// </summary>
    /// <param name="status">The status of the run.</param>
    /// <returns>The run.</returns>
    internal static RebateAccrualRun Run(string status = RebateCodes.RunPosted) =>
        new(
            "r1",
            new DateOnly(2027, 1, 1),
            new DateOnly(2027, 3, 31),
            status,
            20000m,
            200m,
            200m,
            "j1",
            ReversalReason: null,
            [new RebateAccrualLine("p1", "F-1", "TD01", new DateOnly(2027, 2, 1), 21000m), new RebateAccrualLine("p2", "NC-1", "TD04", new DateOnly(2027, 3, 1), -1000m)]);

    /// <summary>
    /// Builds a settlement proposal.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="mode">The execution mode.</param>
    /// <returns>The proposal.</returns>
    internal static RebateSettlementProposal Proposal(SettlementStatus status, RebateExecutionMode mode = RebateExecutionMode.ChargeInvoice) =>
        new(
            "s1",
            AgreementId,
            200m,
            216.2m,
            8.1m,
            16.2m,
            "CHF",
            status,
            mode,
            PassiveInvoiceId: null,
            SetOffAmount: null,
            Version: 2,
            [new RebateSettlementLine("invoice", "F-1", new DateOnly(2027, 2, 1), 21000m, 210m), new RebateSettlementLine("credit_note", "NC-1", new DateOnly(2027, 3, 1), -1000m, -10m)]);

    /// <summary>
    /// Builds the cockpit of the sample agreement.
    /// </summary>
    /// <param name="agreement">The agreement.</param>
    /// <param name="proposals">The proposals.</param>
    /// <returns>The cockpit.</returns>
    internal static RebateAgreementDetail Detail(RebateAgreement agreement, params RebateSettlementProposal[] proposals) =>
        new(agreement, Progress(), [Run()], proposals, new DateTime(2027, 4, 1, 8, 0, 0, DateTimeKind.Utc));

    /// <summary>
    /// Builds a blanket balance.
    /// </summary>
    /// <returns>The balance.</returns>
    internal static BlanketBalance Balance() => SupplierRebateParser.ParseBlanketBalance(BalanceJson);
}
