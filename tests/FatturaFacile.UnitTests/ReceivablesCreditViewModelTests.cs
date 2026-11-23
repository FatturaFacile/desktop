// <copyright file="ReceivablesCreditViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the credit view model: the block status indicator, the exposure, the risk, the limit history, the overrides, the credit check and the limit dialog.
/// </summary>
public sealed class ReceivablesCreditViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = ReceivablesFixtures.CustomerId;

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private ReceivablesLoad<CustomerCreditStatus>? status = ReceivablesFixtures.Load(ReceivablesFixtures.Credit(blocked: true, outcome: CreditOutcome.Block));
    private ReceivablesLoad<CreditDecision>? check;
    private int changed;
    private ReceivablesActionResult result = ReceivablesFixtures.Sent();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesCreditViewModelTests"/> class.
    /// </summary>
    public ReceivablesCreditViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerCreditStatusQuery>(), Any)).ReturnsAsync(() => status);
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerRiskQuery>(), Any)).ReturnsAsync(ReceivablesFixtures.Load(ReceivablesFixtures.Risk()));
        _ = sender.Setup(s => s.Send(It.IsAny<GetCreditLimitsQuery>(), Any)).ReturnsAsync(ReceivablesFixtures.Load<IReadOnlyList<CustomerCreditLimit>>([ReceivablesFixtures.Limit(), ReceivablesFixtures.Limit(granted: 500m, policy: CreditReactionPolicy.Warning)]));
        _ = sender.Setup(s => s.Send(It.IsAny<CheckCustomerCreditQuery>(), Any)).ReturnsAsync(() => check);
        _ = sender.Setup(s => s.Send(It.IsAny<GrantCreditOverrideCommand>(), Any)).ReturnsAsync(() => result);
        _ = sender.Setup(s => s.Send(It.IsAny<RevokeCreditOverrideCommand>(), Any)).ReturnsAsync(() => result);
        _ = sender.Setup(s => s.Send(It.IsAny<DeleteCreditLimitCommand>(), Any)).ReturnsAsync(() => result);
        _ = sender.Setup(s => s.Send(It.IsAny<SaveCreditLimitCommand>(), Any)).ReturnsAsync(() => result);
    }

    /// <summary>
    /// Without a customer the view says to select one and shows nothing else.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoCustomer_ShowsTheSelectionHint()
    {
        var vm = Create();

        await vm.LoadAsync(null, string.Empty, null);

        Assert.False(vm.HasCustomer);
        Assert.Equal(loc["Receivables_SelectCustomer"], vm.StatusMessage);
        Assert.Empty(vm.Limits);
        sender.Verify(s => s.Send(It.IsAny<GetCustomerCreditStatusQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// A blocked customer shows the red indicator with the breaches, the limit figures, the exposure breakdown, the risk factors and the limit history.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_BlockedCustomer_ShowsTheBlockIndicatorTheExposureAndTheRisk()
    {
        var vm = Create();
        var culture = loc.CurrentCulture;

        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        Assert.True(vm.HasCustomer);
        Assert.Equal("Blocked", vm.BlockState);
        Assert.StartsWith(loc["Receivables_Block_Blocked"], vm.BlockText, StringComparison.Ordinal);
        Assert.Contains(loc["Receivables_Breach_over_limit"], vm.BlockText, StringComparison.Ordinal);
        Assert.Equal(ReceivablesFormat.Percent(115m, culture, "-"), vm.UtilizationText);
        Assert.Equal(ReceivablesFormat.Money(-150m, culture), vm.AvailableText);
        Assert.Equal(ReceivablesFormat.Money(900m, culture), vm.OpenInvoicesText);
        Assert.Equal(ReceivablesFormat.Money(100m, culture), vm.DeliveryNotesText);
        Assert.Equal(ReceivablesFormat.Money(200m, culture), vm.OrdersText);
        Assert.Equal(ReceivablesFormat.Money(50m, culture), vm.PaymentsText);
        Assert.Equal(ReceivablesFormat.Money(1150m, culture), vm.ExposureText);
        Assert.True(vm.HasRisk);
        Assert.Equal("Bad", vm.RiskState);
        Assert.Equal(2, vm.Factors.Count);
        Assert.Equal(loc["Receivables_Factor_credit_utilization"], vm.Factors[0].Label);
        Assert.Equal(2, vm.Limits.Count);
        Assert.True(vm.Limits[0].IsCurrent);
    }

    /// <summary>
    /// The indicator is a hold, an amber warning or green depending on the credit position.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_BlockIndicator_FollowsHoldWarningAndOk()
    {
        var vm = Create();

        status = ReceivablesFixtures.Load(ReceivablesFixtures.Credit(hold: true));
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal("Hold", vm.BlockState);
        Assert.Equal(loc["Receivables_Block_Hold"], vm.BlockText);

        status = ReceivablesFixtures.Load(ReceivablesFixtures.Credit(outcome: CreditOutcome.Warning));
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal("Warning", vm.BlockState);

        status = ReceivablesFixtures.Load(ReceivablesFixtures.Credit());
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal("Ok", vm.BlockState);
        Assert.Equal(loc["Receivables_Block_Ok"], vm.BlockText);
    }

    /// <summary>
    /// A customer without a limit shows the no-limit text and dashes for the utilisation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoLimit_ShowsTheNoLimitText()
    {
        status = ReceivablesFixtures.Load(ReceivablesFixtures.Credit() with { Limit = null, UtilizationPercent = null, AvailableCredit = null });
        var vm = Create();

        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        Assert.Equal(loc["Receivables_NoLimit"], vm.LimitText);
        Assert.Equal(loc["Receivables_None"], vm.UtilizationText);
        Assert.Equal(loc["Receivables_None"], vm.AvailableText);
    }

    /// <summary>
    /// A cached position shows the offline notice; without a cache the view says there is no data.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Offline_ShowsTheNotice_AndNoDataWithoutCache()
    {
        status = status! with { FromCache = true };
        var vm = Create();

        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal(loc["Receivables_OfflineCache"], vm.NoticeText);

        status = null;
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal(loc["Receivables_NoData"], vm.StatusMessage);
    }

    /// <summary>
    /// Active overrides can be revoked; revoked ones are listed but cannot.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Overrides_AreListedWithTheirRevokeFlag()
    {
        status = ReceivablesFixtures.Load(ReceivablesFixtures.Credit(overrides: [ReceivablesFixtures.Override(), ReceivablesFixtures.Override(CreditScope.DeliveryNote, revoked: true)]));
        var vm = Create();

        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        Assert.Equal(2, vm.Overrides.Count);
        Assert.True(vm.Overrides[0].CanRevoke);
        Assert.False(vm.Overrides[1].CanRevoke);
        Assert.Contains("Approved by the CFO", vm.Overrides[0].Summary, StringComparison.Ordinal);
    }

    /// <summary>
    /// The credit check shows the outcome, the breaches and that it was decided locally when offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CheckCredit_ShowsTheOutcomeTheBreachesAndTheLocalMark()
    {
        check = ReceivablesFixtures.Load(new CreditDecision(CreditOutcome.Block, ["over_limit"], null, 1200m, 1000m, Overridden: false, CreditReactionPolicy.BlockOrder), fromCache: true);
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        vm.SelectedCheckScope = vm.CheckScopeChoices.First(choice => string.Equals(choice.Key, "delivery_note", StringComparison.Ordinal));
        vm.CheckAmountText = "200";

        await vm.CheckCreditCommand.ExecuteAsync(null);

        Assert.Contains(loc["Receivables_Outcome_block"], vm.CheckText, StringComparison.Ordinal);
        Assert.Contains(loc["Receivables_Breach_over_limit"], vm.CheckText, StringComparison.Ordinal);
        Assert.Contains(loc["Receivables_CheckLocal"], vm.CheckText, StringComparison.Ordinal);
        Assert.Equal(ReceivablesFormat.Bad, vm.CheckState);
        sender.Verify(s => s.Send(It.Is<CheckCustomerCreditQuery>(q => q.Scope == CreditScope.DeliveryNote && q.Amount == 200m && q.CustomerId == CustomerId), Any), Times.Once);
    }

    /// <summary>
    /// An amount that is not a number is refused locally.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CheckCredit_InvalidAmount_SendsNothing()
    {
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        vm.CheckAmountText = "abc";

        await vm.CheckCreditCommand.ExecuteAsync(null);

        Assert.Equal(loc[ReconciliationRules.AmountKey], vm.CheckText);
        sender.Verify(s => s.Send(It.IsAny<CheckCustomerCreditQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// Granting an override sends its scope, reason and expiry, refreshes the views and clears the form.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GrantOverride_SendsTheScopeTheReasonAndTheExpiry()
    {
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        vm.SelectedOverrideScope = vm.OverrideScopeChoices.First(choice => string.Equals(choice.Key, "delivery_note", StringComparison.Ordinal));
        vm.OverrideReason = "Agreed with the CFO";
        vm.OverrideExpires = new DateTime(2027, 12, 31);

        await vm.GrantOverrideCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<GrantCreditOverrideCommand>(c => IsExpiringDeliveryNoteOverride(c)), Any), Times.Once);
        Assert.Equal(1, changed);
        Assert.Equal(string.Empty, vm.OverrideReason);
        Assert.Null(vm.OverrideExpires);
        Assert.Equal(ReceivablesFormat.Good, vm.ActionState);
    }

    /// <summary>
    /// A rejected override keeps the form and shows the localized error.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GrantOverride_Rejected_KeepsTheFormAndShowsTheError()
    {
        result = new ReceivablesActionResult(ReceivablesActionStatus.Rejected, ReceivablesService.ReasonKey);
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        vm.OverrideReason = "no";

        await vm.GrantOverrideCommand.ExecuteAsync(null);

        Assert.Equal(loc[ReceivablesService.ReasonKey], vm.ActionText);
        Assert.Equal("no", vm.OverrideReason);
        Assert.Equal(0, changed);
    }

    /// <summary>
    /// Revoking an override and deleting a limit send the identifiers and refresh the views.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RevokeOverrideAndDeleteLimit_SendTheirRequests()
    {
        var granted = ReceivablesFixtures.Override();
        status = ReceivablesFixtures.Load(ReceivablesFixtures.Credit(overrides: [granted]));
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        await vm.RevokeOverrideCommand.ExecuteAsync(vm.Overrides[0]);
        await vm.DeleteLimitCommand.ExecuteAsync(vm.Limits[1]);

        sender.Verify(s => s.Send(It.Is<RevokeCreditOverrideCommand>(c => c.Request.OverrideId == granted.Id), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<DeleteCreditLimitCommand>(c => c.Request.LimitId == vm.Limits[1].Limit.Id), Any), Times.Once);
        Assert.Equal(2, changed);
    }

    /// <summary>
    /// The new limit button opens an empty dialog, the edit button one with the limit, and close hides it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Editor_NewAndEdit_OpenAndCloseTheLimitDialog()
    {
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        vm.NewLimitCommand.Execute(null);
        Assert.True(vm.IsEditorOpen);
        Assert.False(vm.Editor!.IsEdit);

        vm.EditLimitCommand.Execute(vm.Limits[0]);
        Assert.True(vm.Editor.IsEdit);

        vm.CloseEditorCommand.Execute(null);
        Assert.False(vm.IsEditorOpen);
    }

    /// <summary>
    /// A saved limit closes the dialog and refreshes the views.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Editor_Submit_ClosesTheDialogAndRefreshes()
    {
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        vm.NewLimitCommand.Execute(null);
        vm.Editor!.GrantedText = "2500";

        await vm.Editor.SubmitCommand.ExecuteAsync(null);

        Assert.False(vm.IsEditorOpen);
        Assert.Equal(1, changed);
        sender.Verify(s => s.Send(It.Is<SaveCreditLimitCommand>(c => c.Request.GrantedLimit == 2500m && c.Request.LimitId == null && c.Request.Currency == "EUR"), Any), Times.Once);
    }

    private static bool IsExpiringDeliveryNoteOverride(GrantCreditOverrideCommand command)
    {
        var request = command.Request;
        var what = request.Scope == CreditScope.DeliveryNote && request.ExpiresAt is not null && request.CustomerId == CustomerId;
        var text = string.Equals(request.Reason, "Agreed with the CFO", StringComparison.Ordinal) && string.Equals(request.Reference, "Rossi SA", StringComparison.Ordinal);

        return what && text;
    }

    private ReceivablesCreditViewModel Create()
    {
        time.Advance(new DateTimeOffset(ReceivablesFixtures.Synced) - time.GetUtcNow());

        return new ReceivablesCreditViewModel(sender.Object, loc, time, () =>
        {
            changed++;

            return Task.CompletedTask;
        });
    }
}
