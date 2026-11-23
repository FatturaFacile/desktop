// <copyright file="SupplierRebatesViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Accounting.Queries;
using FatturaFacile.Application.Features.Contacts.Queries;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Wpf.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the supplier rebate cockpit view model: loading from the cache and from the server, the offline and error states, which
/// commands can run, the confirmation of the destructive actions, the settlement workflow and the agreement form.
/// </summary>
public sealed class SupplierRebatesViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime Synced = new(2027, 4, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly SupplierRebatesResult Accepted = new(SupplierRebatesStatus.Synced, Synced);

    private readonly Mock<ISender> sender = new();
    private readonly Mock<ILocalizationService> localization = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierRebatesViewModelTests"/> class.
    /// </summary>
    public SupplierRebatesViewModelTests()
    {
        _ = localization.Setup(l => l[It.IsAny<string>()]).Returns((string key) => "[" + key + "]");
        _ = localization.SetupGet(l => l.CurrentCulture).Returns(CultureInfo.InvariantCulture);

        IReadOnlyList<ContactDto> contacts = [new ContactDto { Id = new Guid(0x6f1d1c0e, 0x0000, 0x4000, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xc1), Type = "supplier", CompanyName = "Rossi SA" }];
        IReadOnlyList<ChartOfAccountDto> accounts = [new ChartOfAccountDto { Id = Guid.NewGuid(), Code = "1300", Name = "Accrued income" }];
        _ = sender.Setup(s => s.Send(It.IsAny<GetContactsQuery>(), Any)).ReturnsAsync(contacts);
        _ = sender.Setup(s => s.Send(It.IsAny<GetChartOfAccountsQuery>(), Any)).ReturnsAsync(accounts);
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketContractsQuery>(), Any)).ReturnsAsync(new RebateCached<IReadOnlyList<BlanketContract>>([], Synced));
        _ = sender.Setup(s => s.Send(It.IsAny<SyncSupplierRebatesCommand>(), Any)).ReturnsAsync(Accepted);
        _ = sender.Setup(s => s.Send(It.IsAny<ChangeRebateAgreementCommand>(), Any)).ReturnsAsync(Accepted);
        _ = sender.Setup(s => s.Send(It.IsAny<SaveRebateAgreementCommand>(), Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Synced, Synced, Data: SupplierRebatesTestData.AgreementJson));
        _ = sender.Setup(s => s.Send(It.IsAny<SaveRebateContributionCommand>(), Any)).ReturnsAsync(Accepted);
        _ = sender.Setup(s => s.Send(It.IsAny<DeleteRebateContributionCommand>(), Any)).ReturnsAsync(Accepted);
        _ = sender.Setup(s => s.Send(It.IsAny<RunRebateAccrualCommand>(), Any)).ReturnsAsync(Accepted);
        _ = sender.Setup(s => s.Send(It.IsAny<ReverseRebateAccrualRunCommand>(), Any)).ReturnsAsync(Accepted);
        _ = sender.Setup(s => s.Send(It.IsAny<ProposeRebateSettlementCommand>(), Any)).ReturnsAsync(Accepted);
        _ = sender.Setup(s => s.Send(It.IsAny<AdvanceRebateSettlementCommand>(), Any)).ReturnsAsync(Accepted);
    }

    /// <summary>
    /// The cockpit shows the agreement from the cache with the live progress: tier, distance to the next tier, projection and scale.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FromCache_ShowsTheAgreementAndTheLiveProgress()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());

        Assert.Equal(SupplierRebatesTestData.AgreementId, viewModel.SelectedAgreement?.Id);
        Assert.True(viewModel.HasDetail);
        Assert.False(viewModel.IsBusy);
        Assert.False(viewModel.IsOffline);
        Assert.Equal("CHF 20,000.00", viewModel.QualifyingText);
        Assert.Equal("CHF 200.00", viewModel.EarnedText);
        Assert.True(viewModel.HasTier);
        Assert.Equal("1.00% / CHF 10,000.00", viewModel.CurrentTierText);
        Assert.Equal("2.00% / CHF 50,000.00 + CHF 100.00", viewModel.NextTierText);
        Assert.Equal("[SupplierRebates_DistanceToNext]", viewModel.DistanceLabel);
        Assert.Equal("CHF 30,000.00", viewModel.DistanceText);
        Assert.Equal("CHF 1,100.00", viewModel.ProjectedAtNextText);
        Assert.Equal("CHF 900.00", viewModel.ProjectedGainText);
        Assert.Equal(25d, viewModel.ProgressPercent);
        Assert.Equal(2, viewModel.Scale.Count);
        Assert.True(viewModel.Scale[0].IsCurrent);
        Assert.False(viewModel.Scale[1].IsCurrent);
        Assert.Equal("[SupplierRebates_OpenEnded]", viewModel.Scale[1].To);
        _ = Assert.Single(viewModel.Contributions);
        _ = Assert.Single(viewModel.Runs);
        _ = Assert.Single(viewModel.Suppliers);
        Assert.Equal(2, viewModel.Accounts.Count);
    }

    /// <summary>
    /// When nothing was ever pulled the cockpit pulls from the server first and then shows the result.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NeverSynced_PullsFromTheServerFirst()
    {
        _ = sender.SetupSequence(s => s.Send(It.IsAny<GetRebateAgreementsQuery>(), Any))
            .ReturnsAsync(new RebateCached<IReadOnlyList<RebateAgreement>>([], SyncedAtUtc: null))
            .ReturnsAsync(new RebateCached<IReadOnlyList<RebateAgreement>>([SupplierRebatesTestData.Agreement()], Synced));
        SetDetail(SupplierRebatesTestData.Agreement());
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<SyncSupplierRebatesCommand>(c => c.Scope == RebateSyncScope.Agreements), Any), Times.Once);
        _ = Assert.Single(viewModel.Agreements);
        Assert.True(viewModel.HasDetail);
    }

    /// <summary>
    /// A cache that is empty after the server answered shows the notice of an empty list instead of an error.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoAgreements_ShowsTheEmptyNotice()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetRebateAgreementsQuery>(), Any)).ReturnsAsync(new RebateCached<IReadOnlyList<RebateAgreement>>([], Synced));
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.Empty(viewModel.Agreements);
        Assert.False(viewModel.HasDetail);
        Assert.Equal("[SupplierRebates_Empty]", viewModel.StatusMessage);
        Assert.Equal("-", viewModel.QualifyingText);
    }

    /// <summary>
    /// Without a connection the sync keeps the cached figures on screen and says so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Offline_KeepsTheCachedFiguresAndFlagsOffline()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        _ = sender.Setup(s => s.Send(It.IsAny<SyncSupplierRebatesCommand>(), Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Offline, Detail: "down"));

        await viewModel.SyncCommand.ExecuteAsync(parameter: null);

        Assert.True(viewModel.IsOffline);
        Assert.Equal("[SupplierRebates_OfflineUsingCache]", viewModel.StatusMessage);
        _ = Assert.Single(viewModel.Agreements);
        Assert.Equal("CHF 30,000.00", viewModel.DistanceText);
        Assert.False(viewModel.IsBusy);
    }

    /// <summary>
    /// A refusal of the permission shows its message and the cockpit stops loading.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_AccessDenied_ShowsTheMessageAndStopsBusy()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketContractsQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("denied"));
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.Equal("denied", viewModel.StatusMessage);
        Assert.False(viewModel.IsBusy);
    }

    /// <summary>
    /// A sync that fails shows the detail; the commands are blocked while it runs and free again afterwards.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WhileRunning_BlocksTheCommandsAndShowsFailures()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        TaskCompletionSource<SupplierRebatesResult> gate = new();
        _ = sender.Setup(s => s.Send(It.IsAny<SyncSupplierRebatesCommand>(), Any)).Returns(gate.Task);

        var running = viewModel.SyncCommand.ExecuteAsync(parameter: null);

        Assert.True(viewModel.IsBusy);
        Assert.False(viewModel.SyncCommand.CanExecute(parameter: null));
        Assert.False(viewModel.RunAccrualCommand.CanExecute(parameter: null));
        Assert.False(viewModel.NewAgreementCommand.CanExecute(parameter: null));

        gate.SetResult(new SupplierRebatesResult(SupplierRebatesStatus.Failed, Detail: "HTTP 500"));
        await running;

        Assert.False(viewModel.IsBusy);
        Assert.True(viewModel.SyncCommand.CanExecute(parameter: null));
        Assert.Equal("[SupplierRebates_SyncFailed]", viewModel.StatusMessage);
    }

    /// <summary>
    /// A draft can be activated, edited and deleted but not closed nor accrued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Commands_ForADraft_AllowActivateEditAndDelete()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(RebateCodes.StatusDraft));

        Assert.True(viewModel.ActivateAgreementCommand.CanExecute(parameter: null));
        Assert.True(viewModel.RequestDeleteAgreementCommand.CanExecute(parameter: null));
        Assert.True(viewModel.EditAgreementCommand.CanExecute(parameter: null));
        Assert.False(viewModel.CloseAgreementCommand.CanExecute(parameter: null));
        Assert.False(viewModel.RunAccrualCommand.CanExecute(parameter: null));
        Assert.False(viewModel.ProposeSettlementCommand.CanExecute(parameter: null));
    }

    /// <summary>
    /// An active agreement can be closed, accrued and settled but not activated nor deleted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Commands_ForAnActiveAgreement_AllowCloseAccrueAndPropose()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());

        Assert.True(viewModel.CloseAgreementCommand.CanExecute(parameter: null));
        Assert.True(viewModel.RunAccrualCommand.CanExecute(parameter: null));
        Assert.True(viewModel.ProposeSettlementCommand.CanExecute(parameter: null));
        Assert.False(viewModel.ActivateAgreementCommand.CanExecute(parameter: null));
        Assert.False(viewModel.RequestDeleteAgreementCommand.CanExecute(parameter: null));
    }

    /// <summary>
    /// Activating sends the command and shows the refreshed cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Activate_SendsTheCommandAndReloadsTheCache()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(RebateCodes.StatusDraft));

        await viewModel.ActivateAgreementCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<ChangeRebateAgreementCommand>(c => c.AgreementId == SupplierRebatesTestData.AgreementId && c.Action == RebateAgreementAction.Activate), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetRebateAgreementsQuery>(), Any), Times.Exactly(2));
        Assert.Equal("[SupplierRebates_ActionDone]", viewModel.StatusMessage);
    }

    /// <summary>
    /// Deleting an agreement waits for the confirmation, which can be dismissed or given.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Delete_WaitsForTheConfirmation()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(RebateCodes.StatusDraft));
        Assert.False(viewModel.ConfirmPendingCommand.CanExecute(parameter: null));

        viewModel.RequestDeleteAgreementCommand.Execute(parameter: null);

        Assert.True(viewModel.HasPendingConfirmation);
        Assert.Equal("[SupplierRebates_ConfirmDelete]", viewModel.PendingConfirmText);
        Assert.True(viewModel.ConfirmPendingCommand.CanExecute(parameter: null));

        viewModel.DismissPendingCommand.Execute(parameter: null);
        Assert.False(viewModel.HasPendingConfirmation);
        sender.Verify(s => s.Send(It.IsAny<ChangeRebateAgreementCommand>(), Any), Times.Never);

        viewModel.RequestDeleteAgreementCommand.Execute(parameter: null);
        await viewModel.ConfirmPendingCommand.ExecuteAsync(parameter: null);

        Assert.False(viewModel.HasPendingConfirmation);
        sender.Verify(s => s.Send(It.Is<ChangeRebateAgreementCommand>(c => c.Action == RebateAgreementAction.Delete), Any), Times.Once);
    }

    /// <summary>
    /// An action that needs the server is not queued offline: the user is told it was not carried out and nothing is reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_Offline_SaysItWasNotCarriedOut()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        _ = sender.Setup(s => s.Send(It.IsAny<RunRebateAccrualCommand>(), Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Offline, Detail: "down"));

        await viewModel.RunAccrualCommand.ExecuteAsync(parameter: null);

        Assert.True(viewModel.IsOffline);
        Assert.Equal("[SupplierRebates_OfflineActionBlocked]", viewModel.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetRebateAgreementsQuery>(), Any), Times.Once);
        Assert.False(viewModel.IsBusy);
    }

    /// <summary>
    /// A business refusal of the server is shown with the message of the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_Rejected_ShowsTheMessageOfTheServer()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        _ = sender.Setup(s => s.Send(It.IsAny<ProposeRebateSettlementCommand>(), Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Rejected, Detail: "Nothing to settle."));

        await viewModel.ProposeSettlementCommand.ExecuteAsync(parameter: null);

        Assert.Equal("Nothing to settle.", viewModel.StatusMessage);
        Assert.False(viewModel.IsOffline);
    }

    /// <summary>
    /// A validation error carries a localization key and is shown translated.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_FailingValidation_ShowsTheTranslatedError()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        _ = sender.Setup(s => s.Send(It.IsAny<ProposeRebateSettlementCommand>(), Any)).ThrowsAsync(new ValidationException([new ValidationFailure("Input.TaxRate", SupplierRebatesErrors.TaxRate)]));

        await viewModel.ProposeSettlementCommand.ExecuteAsync(parameter: null);

        Assert.Equal("[SupplierRebates_Err_TaxRate]", viewModel.StatusMessage);
        Assert.False(viewModel.IsBusy);
    }

    /// <summary>
    /// A proposal is sent with the chosen mode and the VAT rate parsed in the culture of the user.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Propose_SendsTheModeAndTheVatRate()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        viewModel.SelectedMode = viewModel.ExecutionModes.Single(mode => string.Equals(mode.Id, "charge_invoice", StringComparison.Ordinal));
        viewModel.TaxRateText = "8.1";

        await viewModel.ProposeSettlementCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(
                It.Is<ProposeRebateSettlementCommand>(c => c.AgreementId == SupplierRebatesTestData.AgreementId
                    && c.Input.Mode == RebateExecutionMode.ChargeInvoice
                    && c.Input.TaxRate == 8.1m
                    && c.Input.PassiveInvoiceId == null),
                Any),
            Times.Once);
    }

    /// <summary>
    /// Each status of a proposal enables only the steps the state machine allows.
    /// </summary>
    /// <param name="status">The status of the proposal.</param>
    /// <param name="submit">Whether submit is enabled.</param>
    /// <param name="approve">Whether approve is enabled.</param>
    /// <param name="execute">Whether execute is enabled.</param>
    /// <param name="cancel">Whether cancel is enabled.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(SettlementStatus.Draft, true, false, false, true)]
    [InlineData(SettlementStatus.PendingApproval, false, true, false, true)]
    [InlineData(SettlementStatus.Approved, false, false, true, true)]
    [InlineData(SettlementStatus.Settled, false, false, false, false)]
    [InlineData(SettlementStatus.Cancelled, false, false, false, false)]
    public async Task Workflow_ButtonsFollowTheStateMachine(SettlementStatus status, bool submit, bool approve, bool execute, bool cancel)
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(), SupplierRebatesTestData.Proposal(status));

        Assert.True(viewModel.HasProposal);
        Assert.Equal(submit, viewModel.SubmitProposalCommand.CanExecute(parameter: null));
        Assert.Equal(approve, viewModel.ApproveProposalCommand.CanExecute(parameter: null));
        Assert.Equal(execute, viewModel.RequestExecuteProposalCommand.CanExecute(parameter: null));
        Assert.Equal(cancel, viewModel.RequestCancelProposalCommand.CanExecute(parameter: null));
    }

    /// <summary>
    /// The breakdown of the selected proposal lists its invoices and credit notes with their share of the rebate.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Proposal_ShowsTheAnalyticalBreakdown()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(), SupplierRebatesTestData.Proposal(SettlementStatus.Draft));

        Assert.Equal(2, viewModel.ProposalLines.Count);
        Assert.Equal("[SupplierRebates_LineInvoice]", viewModel.ProposalLines[0].Type);
        Assert.Equal("CHF 210.00", viewModel.ProposalLines[0].Share);
        Assert.Equal("[SupplierRebates_LineCreditNote]", viewModel.ProposalLines[1].Type);
        Assert.Equal("CHF -1,000.00", viewModel.ProposalLines[1].Net);
        Assert.Equal("CHF 216.20", viewModel.Proposals[0].Gross);
        Assert.Contains("CHF 16.20", viewModel.ProposalSummaryText, StringComparison.Ordinal);
    }

    /// <summary>
    /// Submit sends the step with the version the user saw and the status the step must be allowed from.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_SendsTheVersionAndTheCurrentStatus()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(), SupplierRebatesTestData.Proposal(SettlementStatus.Draft));

        await viewModel.SubmitProposalCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(
                It.Is<AdvanceRebateSettlementCommand>(c => c.ProposalId == "s1"
                    && c.Action == SettlementAction.Submit
                    && c.CurrentStatus == SettlementStatus.Draft
                    && c.Details.Version == 2),
                Any),
            Times.Once);
    }

    /// <summary>
    /// Executing a set-off settlement asks the confirmation and then sends the ledger accounts that were chosen.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Execute_AfterConfirmation_SendsTheLedgerAccountsOfTheSetOff()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(), SupplierRebatesTestData.Proposal(SettlementStatus.Approved, RebateExecutionMode.ApSetOff));
        viewModel.SelectedPayablesAccount = viewModel.Accounts[1];
        viewModel.SelectedVatAccount = viewModel.Accounts[1];

        viewModel.RequestExecuteProposalCommand.Execute(parameter: null);
        Assert.Equal("[SupplierRebates_ConfirmExecute]", viewModel.PendingConfirmText);
        sender.Verify(s => s.Send(It.IsAny<AdvanceRebateSettlementCommand>(), Any), Times.Never);
        await viewModel.ConfirmPendingCommand.ExecuteAsync(parameter: null);

        var account = viewModel.Accounts[1].Id;
        sender.Verify(
            s => s.Send(
                It.Is<AdvanceRebateSettlementCommand>(c => c.Action == SettlementAction.Execute
                    && c.CurrentStatus == SettlementStatus.Approved
                    && c.Details.PayablesAccountId == account
                    && c.Details.VatAccountId == account),
                Any),
            Times.Once);
    }

    /// <summary>
    /// A set-off can be proposed only against an open payable whose balance covers the gross amount, which follows the VAT rate typed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SetOff_NeedsAPayableThatCoversTheGrossAmount()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        _ = sender.Setup(s => s.Send(It.IsAny<GetRebatePayablesQuery>(), Any)).ReturnsAsync(new RebatePayablesResult(
            SupplierRebatesStatus.Synced,
            [
                new RebatePayableInvoice("p1", "F-1", new DateOnly(2027, 2, 1), 1000m, 400m, "CHF"),
                new RebatePayableInvoice("p2", "F-2", new DateOnly(2027, 3, 1), 150m, 0m, "CHF"),
            ]));

        viewModel.SelectedMode = viewModel.ExecutionModes.Single(mode => string.Equals(mode.Id, "ap_set_off", StringComparison.Ordinal));
        Assert.True(viewModel.IsSetOff);
        Assert.False(viewModel.ProposeSettlementCommand.CanExecute(parameter: null));
        Assert.True(viewModel.LoadPayablesCommand.CanExecute(parameter: null));

        await viewModel.LoadPayablesCommand.ExecuteAsync(parameter: null);

        Assert.Equal(2, viewModel.Payables.Count);
        Assert.True(viewModel.Payables[0].Covers);
        Assert.False(viewModel.Payables[1].Covers);
        viewModel.SelectedPayable = viewModel.Payables[1];
        Assert.False(viewModel.ProposeSettlementCommand.CanExecute(parameter: null));
        viewModel.SelectedPayable = viewModel.Payables[0];
        Assert.True(viewModel.ProposeSettlementCommand.CanExecute(parameter: null));

        viewModel.TaxRateText = "400";
        Assert.False(viewModel.Payables[0].Covers);
        Assert.False(viewModel.ProposeSettlementCommand.CanExecute(parameter: null));

        viewModel.TaxRateText = "8.1";
        await viewModel.ProposeSettlementCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<ProposeRebateSettlementCommand>(c => c.Input.Mode == RebateExecutionMode.ApSetOff && c.Input.PassiveInvoiceId == "p1"), Any), Times.Once);
    }

    /// <summary>
    /// Offline the open payables cannot be read: there are none and the user is told.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadPayables_Offline_ShowsNoRowsAndTheNotice()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        _ = sender.Setup(s => s.Send(It.IsAny<GetRebatePayablesQuery>(), Any)).ReturnsAsync(new RebatePayablesResult(SupplierRebatesStatus.Offline, []));
        viewModel.SelectedMode = viewModel.ExecutionModes.Single(mode => string.Equals(mode.Id, "ap_set_off", StringComparison.Ordinal));

        await viewModel.LoadPayablesCommand.ExecuteAsync(parameter: null);

        Assert.Empty(viewModel.Payables);
        Assert.True(viewModel.IsOffline);
        Assert.Equal("[SupplierRebates_OfflineActionBlocked]", viewModel.PayablesMessage);
    }

    /// <summary>
    /// Selecting a run shows its invoices (credit notes negative) and the journal entry preview, and a reversal needs a reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_ShowsTheInvoicesTheJournalPreviewAndNeedsAReasonToReverse()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());

        viewModel.SelectedRun = viewModel.Runs[0];

        Assert.Equal(2, viewModel.RunLines.Count);
        Assert.Equal("[SupplierRebates_LineCreditNote]", viewModel.RunLines[1].Type);
        Assert.Equal("CHF -1,000.00", viewModel.RunLines[1].Net);
        Assert.True(viewModel.HasJournal);
        Assert.Equal("[SupplierRebates_DebitAccruedIncome] CHF 200.00", viewModel.JournalDebitText);
        Assert.Equal("[SupplierRebates_CreditSupplierIncome] CHF 200.00", viewModel.JournalCreditText);
        Assert.Equal(string.Empty, viewModel.JournalStatusText);
        Assert.False(viewModel.ReverseRunCommand.CanExecute(parameter: null));

        viewModel.ReverseReason = "Wrong period";
        Assert.True(viewModel.ReverseRunCommand.CanExecute(parameter: null));
        await viewModel.ReverseRunCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<ReverseRebateAccrualRunCommand>(c => c.RunId == "r1" && c.Reason == "Wrong period"), Any), Times.Once);
        Assert.Equal(string.Empty, viewModel.ReverseReason);
    }

    /// <summary>
    /// A reversed run cannot be reversed again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Run_Reversed_CannotBeReversedAgain()
    {
        var agreement = SupplierRebatesTestData.Agreement();
        var viewModel = await LoadedAsync(agreement, new RebateAgreementDetail(agreement, SupplierRebatesTestData.Progress(), [SupplierRebatesTestData.Run(RebateCodes.RunReversed)], [], Synced));

        viewModel.SelectedRun = viewModel.Runs[0];
        viewModel.ReverseReason = "Again";

        Assert.False(viewModel.ReverseRunCommand.CanExecute(parameter: null));
        Assert.Equal("[SupplierRebates_RunReversed]", viewModel.Runs[0].Status);
    }

    /// <summary>
    /// The what-if simulation applies the scale of the agreement to a typed spend, and shows a dash for a text that is not a number.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Simulation_AppliesTheScaleToTheTypedSpend()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());

        viewModel.SimulatedVolumeText = "60000";
        Assert.Equal("CHF 1,300.00", viewModel.SimulatedRebateText);

        viewModel.SimulatedVolumeText = "5000";
        Assert.Equal("CHF 0.00", viewModel.SimulatedRebateText);

        viewModel.SimulatedVolumeText = "abc";
        Assert.Equal("-", viewModel.SimulatedRebateText);
    }

    /// <summary>
    /// A contribution is sent with the kind, the amount and the due date typed, and the form is cleared once it is accepted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task AddContribution_SendsTheTypedValuesAndClearsTheForm()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        viewModel.ContributionDescription = " Spring flyer ";
        viewModel.ContributionAmountText = "500";

        await viewModel.AddContributionCommand.ExecuteAsync(parameter: null);
        Assert.Equal("[SupplierRebates_Err_Required]", viewModel.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<SaveRebateContributionCommand>(), Any), Times.Never);

        viewModel.ContributionDue = new DateTime(2027, 6, 30);
        await viewModel.AddContributionCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(
                It.Is<SaveRebateContributionCommand>(c => c.ContributionId == null
                    && IsSpringFlyer(c)
                    && c.Input.DueOn == new DateOnly(2027, 6, 30)),
                Any),
            Times.Once);
        Assert.Equal(string.Empty, viewModel.ContributionAmountText);
        Assert.Null(viewModel.ContributionDue);
    }

    /// <summary>
    /// A planned contribution can be removed, an accrued one cannot.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RemoveContribution_IsOnlyForPlannedContributions()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        Assert.False(viewModel.RemoveContributionCommand.CanExecute(parameter: null));

        viewModel.SelectedContribution = viewModel.Contributions[0];
        Assert.True(viewModel.RemoveContributionCommand.CanExecute(parameter: null));
        await viewModel.RemoveContributionCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<DeleteRebateContributionCommand>(c => c.ContributionId == "k1"), Any), Times.Once);
    }

    /// <summary>
    /// The agreement form opens empty, rejects a number that does not parse, and sends the typed scale once it is valid.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Editor_SavesTheTypedAgreementAndClosesTheForm()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());

        viewModel.NewAgreementCommand.Execute(parameter: null);
        Assert.True(viewModel.IsEditing);
        Assert.False(viewModel.NewAgreementCommand.CanExecute(parameter: null));
        _ = Assert.Single(viewModel.FormTiers);
        Assert.Equal("[SupplierRebates_NewAgreement]", viewModel.EditorTitle);

        viewModel.FormCode = "PFA-1";
        viewModel.FormName = "Rebate";
        viewModel.FormTiers[0].Threshold = "abc";
        viewModel.FormTiers[0].Rate = "1";
        await viewModel.SaveAgreementCommand.ExecuteAsync(parameter: null);
        Assert.True(viewModel.IsEditing);
        Assert.Equal("[SupplierRebates_Err_Amount]", viewModel.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<SaveRebateAgreementCommand>(), Any), Times.Never);

        viewModel.FormTiers[0].Threshold = "10000";
        await viewModel.SaveAgreementCommand.ExecuteAsync(parameter: null);

        sender.Verify(
            s => s.Send(
                It.Is<SaveRebateAgreementCommand>(c => c.AgreementId == null
                    && IsPfa1(c)
                    && c.Input.Tiers.Count == 1
                    && c.Input.Tiers[0] == new RebateTier(10000m, 1m, FlatAmount: null)),
                Any),
            Times.Once);
        Assert.False(viewModel.IsEditing);
    }

    /// <summary>
    /// Editing fills the form with the agreement and sends its version back; the tiers can be added and removed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Editor_EditsTheSelectedAgreementWithItsVersion()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement(RebateCodes.StatusDraft));

        viewModel.EditAgreementCommand.Execute(parameter: null);
        Assert.Equal("PFA-2027", viewModel.FormCode);
        Assert.Equal(2, viewModel.FormTiers.Count);
        Assert.Equal("50000", viewModel.FormTiers[1].Threshold);
        Assert.Equal("100", viewModel.FormTiers[1].Flat);
        Assert.Equal(RebateCodes.FrequencyQuarterly, viewModel.FormFrequency?.Id);

        viewModel.AddTierCommand.Execute(parameter: null);
        Assert.Equal(3, viewModel.FormTiers.Count);
        viewModel.RemoveTierCommand.Execute(viewModel.FormTiers[2]);
        Assert.Equal(2, viewModel.FormTiers.Count);

        await viewModel.SaveAgreementCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<SaveRebateAgreementCommand>(c => c.AgreementId == SupplierRebatesTestData.AgreementId && c.Input.Version == 3 && c.Input.Tiers.Count == 2 && c.Input.Tiers[1].FlatAmount == 100m), Any), Times.Once);
    }

    /// <summary>
    /// Cancelling the form closes it without sending anything.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Editor_Cancel_ClosesTheFormWithoutSaving()
    {
        var viewModel = await LoadedAsync(SupplierRebatesTestData.Agreement());
        viewModel.NewAgreementCommand.Execute(parameter: null);
        Assert.True(viewModel.CancelEditCommand.CanExecute(parameter: null));

        viewModel.CancelEditCommand.Execute(parameter: null);

        Assert.False(viewModel.IsEditing);
        Assert.False(viewModel.CancelEditCommand.CanExecute(parameter: null));
        Assert.True(viewModel.NewAgreementCommand.CanExecute(parameter: null));
        sender.Verify(s => s.Send(It.IsAny<SaveRebateAgreementCommand>(), Any), Times.Never);
    }

    private static bool IsSpringFlyer(SaveRebateContributionCommand c) => string.Equals(c.Input.Kind, RebateCodes.KindMarketing, StringComparison.Ordinal) && string.Equals(c.Input.Description, "Spring flyer", StringComparison.Ordinal) && c.Input.Amount == 500m;

    private static bool IsPfa1(SaveRebateAgreementCommand c) => string.Equals(c.Input.Code, "PFA-1", StringComparison.Ordinal) && string.Equals(c.Input.ContactId, SupplierRebatesTestData.SupplierId, StringComparison.Ordinal) && string.Equals(c.Input.RebateType, RebateCodes.TypeTieredVolume, StringComparison.Ordinal);

    private void SetDetail(RebateAgreement agreement, params RebateSettlementProposal[] proposals) =>
        _ = sender.Setup(s => s.Send(It.IsAny<GetRebateAgreementDetailQuery>(), Any)).ReturnsAsync(SupplierRebatesTestData.Detail(agreement, proposals));

    private SupplierRebatesViewModel Create() => new(sender.Object, localization.Object);

    private async Task<SupplierRebatesViewModel> LoadedAsync(RebateAgreement agreement, params RebateSettlementProposal[] proposals)
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetRebateAgreementsQuery>(), Any)).ReturnsAsync(new RebateCached<IReadOnlyList<RebateAgreement>>([agreement], Synced));
        SetDetail(agreement, proposals);
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        return viewModel;
    }

    private async Task<SupplierRebatesViewModel> LoadedAsync(RebateAgreement agreement, RebateAgreementDetail detail)
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetRebateAgreementsQuery>(), Any)).ReturnsAsync(new RebateCached<IReadOnlyList<RebateAgreement>>([agreement], Synced));
        _ = sender.Setup(s => s.Send(It.IsAny<GetRebateAgreementDetailQuery>(), Any)).ReturnsAsync(detail);
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        return viewModel;
    }
}
