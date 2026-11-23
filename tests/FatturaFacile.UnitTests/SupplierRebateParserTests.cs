// <copyright file="SupplierRebateParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.SupplierRebates;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the mapping of the web-api payloads (decimals as strings, nullable fields) to the read models, and of the request bodies.
/// </summary>
public sealed class SupplierRebateParserTests
{
    /// <summary>
    /// An agreement maps its fields, its scale with the flat amounts and its contributions.
    /// </summary>
    [Fact]
    public void ParseAgreement_MapsTheFieldsTheScaleAndTheContributions()
    {
        var agreement = SupplierRebateParser.ParseAgreement(SupplierRebatesTestData.AgreementJson);

        Assert.Equal("PFA-2027", agreement.Code);
        Assert.Equal("Rossi SA", agreement.ContactName);
        Assert.Equal(RebateTierMode.Retroactive, agreement.TierMode);
        Assert.Equal(new DateOnly(2027, 1, 1), agreement.StartsOn);
        Assert.Equal(new DateOnly(2027, 12, 31), agreement.EndsOn);
        Assert.Equal(RebateCodes.StatusActive, agreement.Status);
        Assert.Equal(RebateCodes.FrequencyQuarterly, agreement.AccrualFrequency);
        Assert.Equal(3, agreement.Version);
        Assert.Equal(250.50m, agreement.OpenRebate);
        Assert.Null(agreement.FixedBonusTarget);
        Assert.True(agreement.HasLedgerAccounts);
        Assert.True(agreement.CanClose);
        Assert.False(agreement.CanActivate);
        Assert.Equal(2, agreement.Tiers.Count);
        Assert.Equal(new RebateTier(50000m, 2m, 100m), agreement.Tiers[1]);
        Assert.Null(agreement.Tiers[0].FlatAmount);
        var contribution = Assert.Single(agreement.Contributions);
        Assert.Equal(RebateCodes.KindCoMarketing, contribution.Kind);
        Assert.Equal(500m, contribution.Amount);
        Assert.Equal(new DateOnly(2027, 6, 30), contribution.DueOn);
    }

    /// <summary>
    /// A list takes every agreement of the array, tolerates a missing payload and numbers sent as JSON numbers.
    /// </summary>
    [Fact]
    public void ParseAgreements_ToleratesMissingAndNumericFields()
    {
        var agreements = SupplierRebateParser.ParseAgreements("[" + SupplierRebatesTestData.AgreementJson + ",{\"id\":\"x\",\"fixed_bonus_amount\":12.5,\"version\":4}]");

        Assert.Equal(2, agreements.Count);
        Assert.Equal("x", agreements[1].Id);
        Assert.Equal(12.5m, agreements[1].FixedBonusAmount);
        Assert.Equal(4, agreements[1].Version);
        Assert.Equal(RebateCodes.StatusDraft, agreements[1].Status);
        Assert.Empty(agreements[1].Tiers);
        Assert.Empty(SupplierRebateParser.ParseAgreements(json: null));
        Assert.Empty(SupplierRebateParser.ParseAgreements("{}"));
        _ = Assert.ThrowsAny<JsonException>(() => SupplierRebateParser.ParseAgreements("not json"));
    }

    /// <summary>
    /// The progress maps the tier position and the contributions, and agrees with the local arithmetic.
    /// </summary>
    [Fact]
    public void ParseProgress_MapsTheTierAndAgreesWithTheCalculator()
    {
        var progress = SupplierRebateParser.ParseProgress(SupplierRebatesTestData.ProgressJson);

        Assert.Equal(20000m, progress.QualifyingVolume);
        Assert.Equal(200m, progress.EarnedRebate);
        Assert.Equal(150m, progress.AccruedRebate);
        Assert.Equal(new DateOnly(2027, 3, 31), progress.AsOf);
        Assert.Null(progress.Bonus);
        Assert.Equal(new RebateContributionSummary(500m, 0m, 0m), progress.Contributions);
        Assert.Equal(2, progress.Scale.Count);
        Assert.Equal(50000m, progress.Scale[0].MaxThreshold);
        Assert.Null(progress.Scale[1].MaxThreshold);
        Assert.NotNull(progress.Tier);
        Assert.Equal(30000m, progress.Tier.DistanceToNext);
        Assert.Equal(progress.Tier, RebateTierCalculator.Progress(SupplierRebatesTestData.Scale, progress.QualifyingVolume, RebateTierMode.Retroactive));
    }

    /// <summary>
    /// A fixed bonus maps the target, the distance and the percentage and has no tier.
    /// </summary>
    [Fact]
    public void ParseProgress_FixedBonus_MapsTheTarget()
    {
        var progress = SupplierRebateParser.ParseProgress(SupplierRebatesTestData.BonusProgressJson);

        Assert.Null(progress.Tier);
        Assert.NotNull(progress.Bonus);
        Assert.Equal(new RebateBonusProgress(30000m, 750m, Reached: false, 5000m, 83.33m), progress.Bonus);
        Assert.Empty(progress.Scale);
    }

    /// <summary>
    /// The runs map their period, their amounts and the signed lines, and know whether they can be reversed.
    /// </summary>
    [Fact]
    public void ParseRuns_MapsTheRunsAndTheirLines()
    {
        var run = Assert.Single(SupplierRebateParser.ParseRuns(SupplierRebatesTestData.RunsJson));

        Assert.True(run.IsPosted);
        Assert.Equal(new DateOnly(2027, 3, 31), run.PeriodEnd);
        Assert.Equal(200m, run.DeltaAmount);
        Assert.Equal("j1", run.JournalEntryId);
        Assert.Equal(2, run.Lines.Count);
        Assert.Equal(-1000m, run.Lines[1].NetAmount);
        Assert.Equal(RebateCodes.CreditNoteDocumentType, run.Lines[1].DocumentType);

        var agreement = SupplierRebateParser.ParseAgreement(SupplierRebatesTestData.AgreementJson);
        Assert.Equal(new RebateJournalPreview(IsPosted: true, 200m, DebitsAccruedIncome: true), run.JournalPreview(agreement));
        Assert.Empty(SupplierRebateParser.ParseRuns(json: null));
    }

    /// <summary>
    /// The proposals map the amounts, the status, the execution mode and the analytical breakdown, and the VAT agrees with the calculator.
    /// </summary>
    [Fact]
    public void ParseProposals_MapsTheStatusTheModeAndTheBreakdown()
    {
        var proposal = Assert.Single(SupplierRebateParser.ParseProposals(SupplierRebatesTestData.ProposalsJson));

        Assert.Equal(SettlementStatus.PendingApproval, proposal.Status);
        Assert.Equal(RebateExecutionMode.ApSetOff, proposal.ExecutionMode);
        Assert.Equal("p9", proposal.PassiveInvoiceId);
        Assert.Equal(2, proposal.Version);
        Assert.Equal(8.10m, proposal.TaxRate);
        Assert.Equal(RebateSettlementCalculator.Tax(proposal.Amount, proposal.TaxRate ?? 0m), proposal.TaxAmount);
        Assert.Equal(RebateSettlementCalculator.Gross(proposal.Amount, proposal.TaxRate ?? 0m), proposal.GrossAmount);
        SettlementAction[] allowed = [SettlementAction.Approve, SettlementAction.Cancel];
        Assert.Equal(allowed, proposal.AllowedActions);
        Assert.Equal(2, proposal.Lines.Count);
        Assert.Equal(new RebateSettlementLine("credit_note", "NC-1", new DateOnly(2027, 3, 1), -1000m, -10m), proposal.Lines[1]);
    }

    /// <summary>
    /// The balance of a blanket agreement maps the ceiling, the minimum, the percentages, the items and the alerts.
    /// </summary>
    [Fact]
    public void ParseBlanketBalance_MapsTheFiguresTheLinesAndTheAlerts()
    {
        var balance = SupplierRebateParser.ParseBlanketBalance(SupplierRebatesTestData.BalanceJson);

        Assert.Equal(100_000m, balance.CeilingAmount);
        Assert.Equal(60000m, balance.MinimumSpendAmount);
        Assert.Equal(85000m, balance.OrderedAmount);
        Assert.Equal(15000m, balance.RemainingCeiling);
        Assert.Equal(85m, balance.CeilingUsedPercent);
        Assert.Equal(0m, balance.MinimumShortfall);
        Assert.Equal(141.67m, balance.MinimumReachedPercent);
        var line = Assert.Single(balance.Lines);
        Assert.Equal(3200m, line.RemainingQuantity);
        Assert.Equal(12.5m, line.UnitPrice);
        Assert.Equal(new BlanketAlert(RebateCodes.AlertCeilingNear, "warning"), Assert.Single(balance.Alerts));
    }

    /// <summary>
    /// A balance without minimum and ceiling keeps the optional figures null.
    /// </summary>
    [Fact]
    public void ParseBlanketBalance_WithoutMinimum_KeepsOptionalFiguresNull()
    {
        var balance = SupplierRebateParser.ParseBlanketBalance("{\"contract_id\":\"k\",\"currency\":\"EUR\",\"ceiling_amount\":\"0.00\",\"minimum_spend_amount\":null,\"ordered_amount\":\"10.00\",\"remaining_ceiling\":null,\"ceiling_used_percent\":null,\"minimum_shortfall\":null,\"minimum_reached_percent\":null,\"lines\":[],\"alerts\":[]}");

        Assert.Null(balance.MinimumSpendAmount);
        Assert.Null(balance.RemainingCeiling);
        Assert.Null(balance.CeilingUsedPercent);
        Assert.Null(balance.MinimumShortfall);
        Assert.Empty(balance.Alerts);
    }

    /// <summary>
    /// The blanket agreement picker maps the number, the title and the supplier.
    /// </summary>
    [Fact]
    public void ParseBlanketContracts_MapsThePicker()
    {
        var contract = Assert.Single(SupplierRebateParser.ParseBlanketContracts(SupplierRebatesTestData.ContractsJson));

        Assert.Equal("RQ-2027-01 - Widgets", contract.Label);
        Assert.Equal(SupplierRebatesTestData.SupplierId, contract.SupplierId);
    }

    /// <summary>
    /// Only the open invoices of the supplier are offered for the set-off: credit notes, settled invoices and other suppliers are left out.
    /// </summary>
    [Fact]
    public void ParsePayables_KeepsTheOpenInvoicesOfTheSupplier()
    {
        var payables = SupplierRebateParser.ParsePayables(SupplierRebatesTestData.PassiveInvoicesJson, SupplierRebatesTestData.SupplierId);

        var invoice = Assert.Single(payables);
        Assert.Equal("p1", invoice.Id);
        Assert.Equal(600m, invoice.OpenBalance);
        Assert.Equal(new DateOnly(2027, 2, 1), invoice.InvoiceDate);
        Assert.Empty(SupplierRebateParser.ParsePayables("[]", SupplierRebatesTestData.SupplierId));
        Assert.Empty(SupplierRebateParser.ParsePayables(json: null, SupplierRebatesTestData.SupplierId));
    }

    /// <summary>
    /// The body of an agreement carries dates as yyyy-MM-dd, decimals as numbers, the scale and nulls for what is not set.
    /// </summary>
    [Fact]
    public void RequestWriter_Agreement_BuildsTheBodyOfTheServer()
    {
        RebateAgreementInput input = new(
            "PFA-2027",
            "Annual rebate",
            SupplierRebatesTestData.SupplierId,
            RebateTierMode.Incremental,
            new DateOnly(2027, 1, 1),
            new DateOnly(2027, 12, 31),
            "CHF",
            RebateCodes.TypeTieredVolume,
            SupplierContractId: null,
            FixedBonusTarget: null,
            FixedBonusAmount: null,
            RebateCodes.FrequencyQuarterly,
            AccrualAccountId: "acc-1",
            IncomeAccountId: null,
            Notes: null,
            SupplierRebatesTestData.Scale,
            Version: 3);

        using var document = JsonDocument.Parse(SupplierRebatesRequestWriter.Agreement(input));
        var root = document.RootElement;

        Assert.Equal("purchase", root.GetProperty("direction").GetString());
        Assert.Equal("incremental", root.GetProperty("tier_mode").GetString());
        Assert.Equal("2027-01-01", root.GetProperty("starts_on").GetString());
        Assert.Equal("2027-12-31", root.GetProperty("ends_on").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("supplier_contract_id").ValueKind);
        Assert.Equal("acc-1", root.GetProperty("accrual_account_id").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("income_account_id").ValueKind);
        Assert.Equal(3, root.GetProperty("version").GetInt32());
        var tiers = root.GetProperty("tiers");
        Assert.Equal(2, tiers.GetArrayLength());
        Assert.Equal(50000m, tiers[1].GetProperty("threshold_amount").GetDecimal());
        Assert.Equal(2m, tiers[1].GetProperty("rate_percent").GetDecimal());
        Assert.Equal(100m, tiers[1].GetProperty("flat_amount").GetDecimal());
        Assert.Equal(JsonValueKind.Null, tiers[0].GetProperty("flat_amount").ValueKind);
    }

    /// <summary>
    /// The bodies of the accrual, the reversal, the proposal and the workflow step carry only what is set.
    /// </summary>
    [Fact]
    public void RequestWriter_OtherBodies_CarryOnlyWhatIsSet()
    {
        Assert.Equal("{}", SupplierRebatesRequestWriter.Accrual(periodEnd: null, periodStart: null));
        Assert.Equal("{\"period_end\":\"2027-03-31\"}", SupplierRebatesRequestWriter.Accrual(new DateOnly(2027, 3, 31), periodStart: null));
        Assert.Equal("{\"reason\":\"Wrong period\"}", SupplierRebatesRequestWriter.Reversal("Wrong period"));
        Assert.Equal(
            "{\"execution_mode\":\"ap_set_off\",\"tax_rate\":null,\"passive_invoice_id\":\"p1\",\"notes\":null}",
            SupplierRebatesRequestWriter.Proposal(new RebateProposalInput(RebateExecutionMode.ApSetOff, TaxRate: null, "p1", Notes: null)));
        Assert.Equal("{\"version\":2}", SupplierRebatesRequestWriter.SettlementStep(new RebateSettlementStep(Version: 2)));
        Assert.Equal(
            "{\"version\":2,\"payables_account_id\":\"pay\",\"vat_account_id\":\"vat\"}",
            SupplierRebatesRequestWriter.SettlementStep(new RebateSettlementStep(2, ExecutedOn: null, "pay", "vat")));
        Assert.Equal(
            "{\"kind\":\"listing\",\"description\":\"Fee\",\"amount\":500.5,\"due_on\":\"2027-06-30\"}",
            SupplierRebatesRequestWriter.Contribution(new RebateContributionInput(RebateCodes.KindListing, "Fee", 500.5m, new DateOnly(2027, 6, 30))));
    }
}
