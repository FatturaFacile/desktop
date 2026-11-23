// <copyright file="ReceivablesLedgerViewModelTests.cs" company="Fattura Facile">
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
/// Tests of the ledger and statement view model: the movements with the running balance, the statement preview, the exports and the email send.
/// </summary>
public sealed class ReceivablesLedgerViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = ReceivablesFixtures.CustomerId;

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly ReceivablesActionResult result = ReceivablesFixtures.Queued();
    private ReceivablesLoad<CustomerLedger>? ledger;
    private ReceivablesLoad<CustomerStatement>? statement;
    private ReceivablesExportResult export = new("C:\\Statements\\statement.pdf", Offline: false);

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesLedgerViewModelTests"/> class.
    /// </summary>
    public ReceivablesLedgerViewModelTests()
    {
        LedgerMovement invoice = new(ReceivablesFixtures.Today.AddDays(-20), null, "invoice", "invoice", "INV-1", null, null, "EUR", 100m, 0m, null, "open", 100m);
        LedgerMovement payment = new(ReceivablesFixtures.Today.AddDays(-5), null, "payment", "payment", "PAY-1", null, null, "EUR", 0m, 40m, null, null, 60m);
        ledger = ReceivablesFixtures.Load(new CustomerLedger([invoice, payment], 60m));
        statement = ReceivablesFixtures.Load(new CustomerStatement("Rossi SA", "Fattura Facile SA", "EUR", ReceivablesFixtures.Today.AddDays(-90), ReceivablesFixtures.Today, ReceivablesFixtures.Today, 0m, [invoice, payment], 60m, [ReceivablesFixtures.Item("INV-1", 60m, 20)], 60m, 60m, new Dictionary<string, decimal>(StringComparer.Ordinal) { ["1_30"] = 60m, ["current"] = 0m }));
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerLedgerQuery>(), Any)).ReturnsAsync(() => ledger);
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerStatementQuery>(), Any)).ReturnsAsync(() => statement);
        _ = sender.Setup(s => s.Send(It.IsAny<ExportCustomerStatementCommand>(), Any)).ReturnsAsync(() => export);
        _ = sender.Setup(s => s.Send(It.IsAny<SendCustomerStatementCommand>(), Any)).ReturnsAsync(() => result);
    }

    /// <summary>
    /// The period starts as the last ninety days.
    /// </summary>
    [Fact]
    public void Constructor_DefaultsToTheLastNinetyDays()
    {
        var vm = Create();

        Assert.Equal(time.GetLocalNow().Date, vm.ToDate);
        Assert.Equal(vm.ToDate.AddDays(-90), vm.FromDate);
    }

    /// <summary>
    /// Without a customer the view says to select one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoCustomer_ShowsTheSelectionHint()
    {
        var vm = Create();

        await vm.LoadAsync(null, string.Empty, null);

        Assert.False(vm.HasCustomer);
        Assert.Equal(loc["Receivables_SelectCustomer"], vm.StatusMessage);
        Assert.Empty(vm.Rows);
        sender.Verify(s => s.Send(It.IsAny<GetCustomerLedgerQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// The ledger lists the movements with the running balance, and the statement preview shows the balances and the open items.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheMovementsTheRunningBalanceAndTheStatementPreview()
    {
        var vm = Create();
        var culture = loc.CurrentCulture;

        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        Assert.Equal(2, vm.Rows.Count);
        Assert.Equal(ReceivablesFormat.Money(100m, culture), vm.Rows[0].DebitText);
        Assert.Equal(string.Empty, vm.Rows[0].CreditText);
        Assert.Equal(ReceivablesFormat.Money(40m, culture), vm.Rows[1].CreditText);
        Assert.Equal(ReceivablesFormat.Money(60m, culture), vm.Rows[1].BalanceText);
        Assert.Equal("INV-1", vm.Rows[0].Reference);
        Assert.Equal(string.Format(culture, loc["Receivables_ClosingBalance"], ReceivablesFormat.Money(60m, culture)), vm.ClosingText);
        Assert.True(vm.HasStatement);
        Assert.Contains("Rossi SA", vm.StatementHeader, StringComparison.Ordinal);
        Assert.Equal(string.Format(culture, loc["Receivables_StatementOpen"], "1", ReceivablesFormat.Money(60m, culture), ReceivablesFormat.Money(60m, culture)), vm.StatementOpen);
        Assert.Contains("1_30", vm.StatementAging, StringComparison.Ordinal);
        Assert.DoesNotContain("current", vm.StatementAging, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.Is<GetCustomerLedgerQuery>(q => q.CustomerId == CustomerId && q.From == DateOnly.FromDateTime(vm.FromDate) && q.To == DateOnly.FromDateTime(vm.ToDate)), Any), Times.Once);
    }

    /// <summary>
    /// A cached ledger shows the offline notice; an empty ledger says there are no movements; no cache says there is no data.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Offline_ShowsTheNoticeAndTheEmptyMessage()
    {
        var vm = Create();
        ledger = ledger! with { FromCache = true };
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal(loc["Receivables_OfflineCache"], vm.NoticeText);

        ledger = ReceivablesFixtures.Load(new CustomerLedger([], 0m));
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal(loc["Receivables_NoMovements"], vm.StatusMessage);

        ledger = null;
        statement = null;
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        Assert.Equal(loc["Receivables_NoData"], vm.StatusMessage);
        Assert.False(vm.HasStatement);
    }

    /// <summary>
    /// The PDF and Excel exports ask for their format and show where the file was saved.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Export_Pdf_AndExcel_ShowThePathOfTheSavedFile()
    {
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        await vm.ExportPdfCommand.ExecuteAsync(null);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["Receivables_Exported"], "C:\\Statements\\statement.pdf"), vm.ResultText);
        Assert.Equal(ReceivablesFormat.Good, vm.ResultState);

        await vm.ExportExcelCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<ExportCustomerStatementCommand>(c => c.Format == "pdf" && c.CustomerId == CustomerId), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<ExportCustomerStatementCommand>(c => c.Format == "xlsx"), Any), Times.Once);
    }

    /// <summary>
    /// When the server cannot be reached the export says so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Export_Offline_ShowsTheOfflineNotice()
    {
        export = new ReceivablesExportResult(null, Offline: true);
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");

        await vm.ExportPdfCommand.ExecuteAsync(null);

        Assert.Equal(loc["Receivables_ExportOffline"], vm.ResultText);
        Assert.Equal(ReceivablesFormat.Warn, vm.ResultState);
    }

    /// <summary>
    /// Sending the statement uses the language of the application, a trimmed recipient or none, and shows the queued notice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SendStatement_UsesTheLanguageOfTheApplicationAndTheRecipient()
    {
        var vm = Create();
        await vm.LoadAsync(CustomerId, "Rossi SA", "EUR");
        var expectedLocale = loc.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "de" => "de",
            "fr" => "fr",
            "it" => "it",
            _ => "en",
        };

        vm.Recipient = "  accounts@rossi.example ";
        await vm.SendStatementCommand.ExecuteAsync(null);
        vm.Recipient = string.Empty;
        await vm.SendStatementCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<SendCustomerStatementCommand>(c => c.Request.Recipient == "accounts@rossi.example" && c.Request.Locale == expectedLocale && c.Request.CustomerId == CustomerId), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<SendCustomerStatementCommand>(c => c.Request.Recipient == null), Any), Times.Once);
        Assert.Equal(loc["Receivables_Queued"], vm.ResultText);
        Assert.Equal(ReceivablesFormat.Warn, vm.ResultState);
    }

    private ReceivablesLedgerViewModel Create() => new(sender.Object, loc, time);
}
