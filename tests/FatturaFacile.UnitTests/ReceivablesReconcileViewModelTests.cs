// <copyright file="ReceivablesReconcileViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the reconciliation dialog: the kinds, the payments to take from, the preview, the local validation and the submit.
/// </summary>
public sealed class ReceivablesReconcileViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = ReceivablesFixtures.CustomerId;

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly ReceivableItem item = ReceivablesFixtures.Item("INV-1", 100m, 5, original: 100m);
    private readonly UnreconciledPayment payment = new(Guid.NewGuid(), "BANK-1", ReceivablesFixtures.Today, "EUR", 500m, 60m);
    private int completed;
    private ReceivablesActionResult result = ReceivablesFixtures.Sent();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesReconcileViewModelTests"/> class.
    /// </summary>
    public ReceivablesReconcileViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<ApplyReceivableAllocationsCommand>(), Any)).ReturnsAsync(() => result);
    }

    private ReceivablesReconcileViewModel Create() => new(sender.Object, loc, time, item, [payment], () =>
    {
        completed++;

        return Task.CompletedTask;
    });

    /// <summary>
    /// The dialog starts on a payment, offers the five allocation kinds and the payment to take the amount from.
    /// </summary>
    [Fact]
    public void Constructor_OffersTheFiveAllocationKindsAndTheUnreconciledPayments()
    {
        var vm = Create();

        Assert.Equal("payment", vm.SelectedType?.Key);
        Assert.Equal(["payment", "credit_note", "debit_adjustment", "write_off", "discount"], vm.TypeChoices.Select(choice => choice.Key));
        Assert.Equal(2, vm.PaymentChoices.Count);
        Assert.True(vm.ShowPayments);
        Assert.False(vm.ShowCreditNote);
        Assert.Equal(time.GetLocalNow().Date, vm.ValueDate);
        Assert.Equal("INV-1", vm.DocumentNumber);
    }

    /// <summary>
    /// The full button fills the open amount of the item.
    /// </summary>
    [Fact]
    public void SetFull_FillsTheOpenAmount()
    {
        var vm = Create();

        vm.SetFullCommand.Execute(null);

        Assert.Equal(ReceivablesFormat.Money(100m, loc.CurrentCulture), vm.AmountText);
    }

    /// <summary>
    /// A partial amount previews the open amount and the partially paid status; a full one previews paid.
    /// </summary>
    [Fact]
    public void Preview_ShowsTheOpenAmountAndStatusAfterThePayment()
    {
        var vm = Create();
        var culture = loc.CurrentCulture;

        vm.AmountText = "40";
        Assert.Equal(string.Format(culture, loc["Receivables_PreviewOpen"], ReceivablesFormat.Money(60m, culture), loc["Receivables_Status_partially_paid"]), vm.PreviewText);

        vm.AmountText = "100";
        Assert.Equal(string.Format(culture, loc["Receivables_PreviewOpen"], ReceivablesFormat.Money(0m, culture), loc["Receivables_Status_paid"]), vm.PreviewText);
    }

    /// <summary>
    /// An amount over the open amount shows the localized error instead of a preview.
    /// </summary>
    [Fact]
    public void Preview_AmountOverTheOpenAmount_ShowsTheError()
    {
        var vm = Create();

        vm.AmountText = "150";

        Assert.Equal(loc[ReconciliationRules.ExceedsKey], vm.PreviewText);
    }

    /// <summary>
    /// Choosing a payment proposes the smaller of the open amount and what the payment still has.
    /// </summary>
    [Fact]
    public void SelectPayment_FillsTheSmallerOfTheOpenAmountAndWhatTheyHaveLeft()
    {
        var vm = Create();

        vm.SelectedPayment = vm.PaymentChoices[1];

        Assert.Equal(ReceivablesFormat.Money(60m, loc.CurrentCulture), vm.AmountText);
    }

    /// <summary>
    /// Submitting sends the allocation with the ledger entry of the chosen payment, shows the outcome and refreshes the views.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_PaymentFromAnUnreconciledPayment_SendsTheLineWithTheSource()
    {
        var vm = Create();
        vm.SelectedPayment = vm.PaymentChoices[1];
        vm.AmountText = "60";
        vm.Reference = "r1";

        await vm.SubmitCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<ApplyReceivableAllocationsCommand>(c =>
            c.Request.CustomerId == CustomerId
            && c.Request.Lines.Count == 1
            && c.Request.Lines[0].ReceivableItemId == item.Id
            && c.Request.Lines[0].Type == AllocationType.Payment
            && c.Request.Lines[0].Amount == 60m
            && c.Request.Lines[0].SourceId == payment.LedgerEntryId.ToString("D", CultureInfo.InvariantCulture)
            && c.Request.Lines[0].Reference == "r1"), Any), Times.Once);
        Assert.Equal(loc["Receivables_Sent"], vm.ResultText);
        Assert.Equal(ReceivablesFormat.Good, vm.ResultState);
        Assert.Equal(1, completed);
    }

    /// <summary>
    /// A text that is not an amount is refused locally.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_InvalidAmount_SendsNothing()
    {
        var vm = Create();
        vm.AmountText = "abc";

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(loc[ReconciliationRules.AmountKey], vm.ResultText);
        Assert.Equal(ReceivablesFormat.Bad, vm.ResultState);
        sender.Verify(s => s.Send(It.IsAny<ApplyReceivableAllocationsCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// An amount over what the chosen payment has left is refused before anything is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_AmountOverTheChosenPayment_IsRefusedLocally()
    {
        var vm = Create();
        vm.SelectedPayment = vm.PaymentChoices[1];
        vm.AmountText = "80";

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(loc[ReconciliationRules.PaymentExceedsKey], vm.ResultText);
        sender.Verify(s => s.Send(It.IsAny<ApplyReceivableAllocationsCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A credit note shows its id field and sends the trimmed id as the source.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_CreditNote_SendsTheCreditNoteId()
    {
        var vm = Create();
        vm.SelectedType = vm.TypeChoices.First(choice => choice.Key == "credit_note");
        Assert.True(vm.ShowCreditNote);
        Assert.False(vm.ShowPayments);
        vm.CreditNoteId = " cn-1 ";
        vm.AmountText = "30";

        await vm.SubmitCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<ApplyReceivableAllocationsCommand>(c => c.Request.Lines[0].Type == AllocationType.CreditNote && c.Request.Lines[0].SourceId == "cn-1" && c.Request.Lines[0].Amount == 30m), Any), Times.Once);
    }

    /// <summary>
    /// A debit adjustment may exceed the open amount because it raises it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_DebitAdjustment_IsAllowedBeyondTheOpenAmount()
    {
        var vm = Create();
        vm.SelectedType = vm.TypeChoices.First(choice => choice.Key == "debit_adjustment");
        vm.AmountText = "500";

        await vm.SubmitCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<ApplyReceivableAllocationsCommand>(c => c.Request.Lines[0].Type == AllocationType.DebitAdjustment && c.Request.Lines[0].Amount == 500m && c.Request.Lines[0].SourceId == null), Any), Times.Once);
    }

    /// <summary>
    /// Offline, the allocation is queued: the notice is amber and the views are refreshed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_Queued_ShowsTheQueuedNoticeAndStillRefreshes()
    {
        result = ReceivablesFixtures.Queued();
        var vm = Create();
        vm.AmountText = "10";

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(loc["Receivables_Queued"], vm.ResultText);
        Assert.Equal(ReceivablesFormat.Warn, vm.ResultState);
        Assert.Equal(1, completed);
    }

    /// <summary>
    /// A refusal of the server shows its reason and does not refresh the views.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_Refused_ShowsTheServerReasonAndDoesNotRefresh()
    {
        result = new ReceivablesActionResult(ReceivablesActionStatus.Conflict, Detail: "Already paid.");
        var vm = Create();
        vm.AmountText = "10";

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Contains("Already paid.", vm.ResultText, StringComparison.Ordinal);
        Assert.Equal(ReceivablesFormat.Bad, vm.ResultState);
        Assert.Equal(0, completed);
    }
}
