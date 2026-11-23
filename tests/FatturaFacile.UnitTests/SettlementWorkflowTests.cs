// <copyright file="SettlementWorkflowTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;
using FluentValidation.TestHelper;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the settlement proposal state machine (draft, pending approval, approved, settled, cancelled) and of the validators of
/// the supplier rebate commands, which refuse what the server would refuse.
/// </summary>
public sealed class SettlementWorkflowTests
{
    /// <summary>
    /// Gets the allowed actions of every status.
    /// </summary>
    public static TheoryData<SettlementStatus, SettlementAction[]> AllowedByStatus => new()
    {
        { SettlementStatus.Draft, [SettlementAction.Submit, SettlementAction.Cancel] },
        { SettlementStatus.PendingApproval, [SettlementAction.Approve, SettlementAction.Cancel] },
        { SettlementStatus.Approved, [SettlementAction.Execute, SettlementAction.Cancel] },
        { SettlementStatus.Settled, [] },
        { SettlementStatus.Cancelled, [] },
    };

    /// <summary>
    /// Each status accepts only its own steps: submit then approve then execute, and cancel until the proposal is settled.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="expected">The expected allowed actions.</param>
    [Theory]
    [MemberData(nameof(AllowedByStatus))]
    public void AllowedActions_FollowTheWorkflow(SettlementStatus status, SettlementAction[] expected) =>
        Assert.Equal(expected, SettlementWorkflow.AllowedActions(status));

    /// <summary>
    /// A step out of order is refused and has no next status.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="action">The action.</param>
    [Theory]
    [InlineData(SettlementStatus.Draft, SettlementAction.Approve)]
    [InlineData(SettlementStatus.Draft, SettlementAction.Execute)]
    [InlineData(SettlementStatus.PendingApproval, SettlementAction.Submit)]
    [InlineData(SettlementStatus.PendingApproval, SettlementAction.Execute)]
    [InlineData(SettlementStatus.Approved, SettlementAction.Submit)]
    [InlineData(SettlementStatus.Approved, SettlementAction.Approve)]
    [InlineData(SettlementStatus.Settled, SettlementAction.Cancel)]
    [InlineData(SettlementStatus.Cancelled, SettlementAction.Submit)]
    public void Advance_OutOfOrder_IsRefused(SettlementStatus status, SettlementAction action)
    {
        Assert.False(SettlementWorkflow.CanApply(status, action));
        Assert.Null(SettlementWorkflow.Advance(status, action));
    }

    /// <summary>
    /// A step in order moves to the next status.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="action">The action.</param>
    /// <param name="expected">The expected next status.</param>
    [Theory]
    [InlineData(SettlementStatus.Draft, SettlementAction.Submit, SettlementStatus.PendingApproval)]
    [InlineData(SettlementStatus.PendingApproval, SettlementAction.Approve, SettlementStatus.Approved)]
    [InlineData(SettlementStatus.Approved, SettlementAction.Execute, SettlementStatus.Settled)]
    [InlineData(SettlementStatus.Draft, SettlementAction.Cancel, SettlementStatus.Cancelled)]
    [InlineData(SettlementStatus.PendingApproval, SettlementAction.Cancel, SettlementStatus.Cancelled)]
    [InlineData(SettlementStatus.Approved, SettlementAction.Cancel, SettlementStatus.Cancelled)]
    public void Advance_InOrder_MovesToTheNextStatus(SettlementStatus status, SettlementAction action, SettlementStatus expected)
    {
        Assert.True(SettlementWorkflow.CanApply(status, action));
        Assert.Equal(expected, SettlementWorkflow.Advance(status, action));
    }

    /// <summary>
    /// The codes of the web-api map to the statuses, the modes and the path segments, and back.
    /// </summary>
    [Fact]
    public void Wire_MapsTheCodesOfTheWebApi()
    {
        Assert.Equal(SettlementStatus.PendingApproval, RebateWire.ParseStatus("pending_approval"));
        Assert.Equal(SettlementStatus.Draft, RebateWire.ParseStatus("something-new"));
        Assert.Equal("pending_approval", RebateWire.ToWire(SettlementStatus.PendingApproval));
        Assert.Equal(RebateExecutionMode.ApSetOff, RebateWire.ParseMode("ap_set_off"));
        Assert.Null(RebateWire.ParseMode(code: null));
        Assert.Equal("credit_note_request", RebateWire.ToWire(RebateExecutionMode.CreditNoteRequest));
        Assert.Equal("execute", RebateWire.ToWire(SettlementAction.Execute));
        Assert.Equal(RebateTierMode.Incremental, RebateWire.ParseTierMode("incremental"));
        Assert.Equal(RebateTierMode.Retroactive, RebateWire.ParseTierMode("unknown"));
    }

    /// <summary>
    /// A step that the state machine refuses fails the validation with the localization key of the error.
    /// </summary>
    [Fact]
    public void AdvanceValidator_RefusesAStepOutOfOrder()
    {
        AdvanceRebateSettlementCommandValidator validator = new();

        var refused = validator.TestValidate(new AdvanceRebateSettlementCommand("a1", "s1", SettlementStatus.Draft, SettlementAction.Execute, new RebateSettlementStep(Version: 1)));
        var accepted = validator.TestValidate(new AdvanceRebateSettlementCommand("a1", "s1", SettlementStatus.Approved, SettlementAction.Execute, new RebateSettlementStep(Version: 1)));

        Assert.Contains(refused.Errors, error => string.Equals(error.ErrorMessage, SupplierRebatesErrors.ActionNotAllowed, StringComparison.Ordinal));
        accepted.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// A proposal needs a VAT rate unless it sets off against a payable, and a set-off needs the payable.
    /// </summary>
    [Fact]
    public void ProposeValidator_AsksForTheVatRateAndThePayable()
    {
        ProposeRebateSettlementCommandValidator validator = new();

        var noRate = validator.TestValidate(new ProposeRebateSettlementCommand("a1", new RebateProposalInput(RebateExecutionMode.ChargeInvoice, TaxRate: null, PassiveInvoiceId: null, Notes: null)));
        var noInvoice = validator.TestValidate(new ProposeRebateSettlementCommand("a1", new RebateProposalInput(RebateExecutionMode.ApSetOff, TaxRate: null, PassiveInvoiceId: null, Notes: null)));
        var setOff = validator.TestValidate(new ProposeRebateSettlementCommand("a1", new RebateProposalInput(RebateExecutionMode.ApSetOff, TaxRate: null, "p1", Notes: null)));
        var badRate = validator.TestValidate(new ProposeRebateSettlementCommand("a1", new RebateProposalInput(RebateExecutionMode.CreditNoteRequest, TaxRate: 101m, PassiveInvoiceId: null, Notes: null)));

        _ = noRate.ShouldHaveValidationErrorFor(c => c.Input.TaxRate).WithErrorMessage(SupplierRebatesErrors.TaxRate);
        _ = noInvoice.ShouldHaveValidationErrorFor(c => c.Input.PassiveInvoiceId).WithErrorMessage(SupplierRebatesErrors.InvoiceRequired);
        setOff.ShouldNotHaveAnyValidationErrors();
        _ = badRate.ShouldHaveValidationErrorFor(c => c.Input.TaxRate).WithErrorMessage(SupplierRebatesErrors.TaxRate);
    }

    /// <summary>
    /// An agreement needs its code, its dates in order, its scale with distinct thresholds, and for a fixed bonus the target and the amount.
    /// </summary>
    [Fact]
    public void AgreementValidator_AppliesTheRulesOfTheServer()
    {
        SaveRebateAgreementCommandValidator validator = new();
        var valid = Agreement(RebateCodes.TypeTieredVolume, [new RebateTier(0m, 1m), new RebateTier(1000m, 2m)]);

        validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, valid)).ShouldNotHaveAnyValidationErrors();
        _ = validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, valid with { Code = " " })).ShouldHaveValidationErrorFor(c => c.Input.Code).WithErrorMessage(SupplierRebatesErrors.Required);
        _ = validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, valid with { EndsOn = valid.StartsOn.AddDays(-1) })).ShouldHaveValidationErrorFor(c => c.Input.EndsOn).WithErrorMessage(SupplierRebatesErrors.DatesOrder);
        _ = validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, valid with { Tiers = [] })).ShouldHaveValidationErrorFor(c => c.Input.Tiers).WithErrorMessage(SupplierRebatesErrors.TiersRequired);
        _ = validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, valid with { Tiers = [new RebateTier(500m, 1m), new RebateTier(500m, 2m)] })).ShouldHaveValidationErrorFor(c => c.Input.Tiers).WithErrorMessage(SupplierRebatesErrors.TiersDistinct);
        _ = validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, valid with { Tiers = [new RebateTier(0m, 101m)] })).ShouldHaveValidationErrorFor("Input.Tiers[0].RatePercent");
        _ = validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, valid with { Currency = "EURO" })).ShouldHaveValidationErrorFor(c => c.Input.Currency).WithErrorMessage(SupplierRebatesErrors.Currency);

        var bonus = valid with { RebateType = RebateCodes.TypeFixedBonus, Tiers = [] };
        _ = validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, bonus)).ShouldHaveValidationErrorFor(c => c.Input.FixedBonusTarget).WithErrorMessage(SupplierRebatesErrors.BonusRequired);
        validator.TestValidate(new SaveRebateAgreementCommand(AgreementId: null, bonus with { FixedBonusTarget = 30000m, FixedBonusAmount = 750m })).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// A contribution needs a known kind and a positive amount, and a reversal needs a reason.
    /// </summary>
    [Fact]
    public void ContributionAndReversalValidators_AskForTheirFields()
    {
        SaveRebateContributionCommandValidator contribution = new();
        ReverseRebateAccrualRunCommandValidator reversal = new();
        RebateContributionInput valid = new(RebateCodes.KindListing, "Listing fee", 500m, new DateOnly(2027, 6, 30));

        contribution.TestValidate(new SaveRebateContributionCommand("a1", ContributionId: null, valid)).ShouldNotHaveAnyValidationErrors();
        _ = contribution.TestValidate(new SaveRebateContributionCommand("a1", ContributionId: null, valid with { Kind = "gift" })).ShouldHaveValidationErrorFor(c => c.Input.Kind).WithErrorMessage(SupplierRebatesErrors.Kind);
        _ = contribution.TestValidate(new SaveRebateContributionCommand("a1", ContributionId: null, valid with { Amount = 0m })).ShouldHaveValidationErrorFor(c => c.Input.Amount).WithErrorMessage(SupplierRebatesErrors.Amount);
        _ = reversal.TestValidate(new ReverseRebateAccrualRunCommand("a1", "r1", " ")).ShouldHaveValidationErrorFor(c => c.Reason).WithErrorMessage(SupplierRebatesErrors.Reason);
        reversal.TestValidate(new ReverseRebateAccrualRunCommand("a1", "r1", "Wrong period")).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// The accrual period must start before it ends.
    /// </summary>
    [Fact]
    public void AccrualValidator_RefusesAPeriodThatEndsBeforeItStarts()
    {
        RunRebateAccrualCommandValidator validator = new();

        validator.TestValidate(new RunRebateAccrualCommand("a1")).ShouldNotHaveAnyValidationErrors();
        _ = validator.TestValidate(new RunRebateAccrualCommand("a1", new DateOnly(2027, 1, 1), new DateOnly(2027, 2, 1))).ShouldHaveValidationErrorFor(c => c.PeriodStart).WithErrorMessage(SupplierRebatesErrors.DatesOrder);
    }

    private static RebateAgreementInput Agreement(string type, IReadOnlyList<RebateTier> tiers) =>
        new(
            "PFA-2027",
            "Annual rebate",
            "c1",
            RebateTierMode.Retroactive,
            new DateOnly(2027, 1, 1),
            new DateOnly(2027, 12, 31),
            "CHF",
            type,
            SupplierContractId: null,
            FixedBonusTarget: null,
            FixedBonusAmount: null,
            RebateCodes.FrequencyMonthly,
            AccrualAccountId: null,
            IncomeAccountId: null,
            Notes: null,
            tiers);
}
