// <copyright file="HelpdeskTimeLogViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the time log dialog: local validation, the request it sends, and how the settlement (contract deduction, billable,
/// estimate while queued, refusal) is shown.
/// </summary>
public sealed class HelpdeskTimeLogViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly HelpdeskTicketRisk Ticket = new(Guid.NewGuid(), "TCK-7", "Printer", "high", "open", null, null, null, 90, []);

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly List<LogTicketTimeCommand> sent = [];
    private HelpdeskActionResult result = new(HelpdeskActionStatus.Sent);
    private int completed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpdeskTimeLogViewModelTests"/> class.
    /// </summary>
    public HelpdeskTimeLogViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<LogTicketTimeCommand>(), Any))
            .Callback<IRequest<HelpdeskActionResult>, CancellationToken>((command, _) => sent.Add((LogTicketTimeCommand)command))
            .ReturnsAsync(() => result);
    }

    /// <summary>
    /// A minutes text that is not a number is refused in the dialog and nothing is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_WithoutMinutes_ShowsTheErrorAndSendsNothing()
    {
        var vm = Create(remaining: null);
        vm.MinutesText = "abc";

        await vm.SubmitAsync();

        Assert.Equal("Bad", vm.ResultState);
        Assert.Equal(loc[HelpdeskService.MinutesKey], vm.ResultText);
        Assert.Empty(sent);
        Assert.Equal(0, completed);
    }

    /// <summary>
    /// The request carries the minutes, the description, the billable flag, the contract bank and the instant of the agent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_SendsTheEntryWithTheBankAndRefreshesTheWorkbench()
    {
        var vm = Create(remaining: 1.5m);
        vm.MinutesText = "45";
        vm.Description = "Replaced the fuser";
        vm.IsBillable = false;

        await vm.SubmitAsync();

        var request = Assert.Single(sent).Request;
        Assert.Equal(Ticket.TicketId, request.TicketId);
        Assert.Equal("TCK-7", request.TicketNumber);
        Assert.Equal(45, request.Minutes);
        Assert.Equal("Replaced the fuser", request.Description);
        Assert.False(request.IsBillable);
        Assert.Equal(1.5m, request.RemainingContractHours);
        Assert.Equal(time.GetUtcNow().UtcDateTime, request.OccurredAtUtc);
        Assert.Equal(1, completed);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// The server answer shows how much was deducted from the contract and how much is billable.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_Sent_ShowsContractDeductionAndBillableHours()
    {
        result = new HelpdeskActionResult(HelpdeskActionStatus.Sent, TimeOutcome: new HelpdeskTimeOutcome(HelpdeskTimeOutcome.PartiallyDeducted, 0.25m, 0.75m, 60m));
        var vm = Create(remaining: 0.25m);
        vm.MinutesText = "60";

        await vm.SubmitAsync();

        var culture = loc.CurrentCulture;
        Assert.Equal("Good", vm.ResultState);
        Assert.Equal(string.Format(culture, loc["Helpdesk_Time_Partial"], 0.25m.ToString("0.##", culture), 0.75m.ToString("0.##", culture)), vm.ResultText);
    }

    /// <summary>
    /// Each billing status has its own message: fully deducted, billable with the charge, not billable, or none known.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_Sent_DescribesEveryBillingStatus()
    {
        var vm = Create(remaining: null);
        vm.MinutesText = "30";
        var culture = loc.CurrentCulture;

        result = new HelpdeskActionResult(HelpdeskActionStatus.Sent, TimeOutcome: new HelpdeskTimeOutcome(HelpdeskTimeOutcome.ContractDeducted, 0.5m, 0m, 0m));
        await vm.SubmitAsync();
        Assert.Equal(string.Format(culture, loc["Helpdesk_Time_ContractDeducted"], 0.5m.ToString("0.##", culture)), vm.ResultText);

        result = new HelpdeskActionResult(HelpdeskActionStatus.Sent, TimeOutcome: new HelpdeskTimeOutcome(HelpdeskTimeOutcome.BillablePending, 0m, 0.5m, 40m));
        await vm.SubmitAsync();
        Assert.StartsWith(string.Format(culture, loc["Helpdesk_Time_Billable"], 0.5m.ToString("0.##", culture)), vm.ResultText, StringComparison.Ordinal);
        Assert.Contains(string.Format(culture, loc["Helpdesk_Time_Charge"], 40m.ToString("N2", culture)), vm.ResultText, StringComparison.Ordinal);

        result = new HelpdeskActionResult(HelpdeskActionStatus.Sent, TimeOutcome: new HelpdeskTimeOutcome(HelpdeskTimeOutcome.NonBillable, 0m, 0m, 0m));
        await vm.SubmitAsync();
        Assert.Equal(loc["Helpdesk_Time_NonBillable"], vm.ResultText);

        result = new HelpdeskActionResult(HelpdeskActionStatus.Sent);
        await vm.SubmitAsync();
        Assert.Equal(loc["Helpdesk_Time_Logged"], vm.ResultText);
    }

    /// <summary>
    /// Offline, the dialog says the entry is queued and shows the local estimate.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_Queued_ShowsTheQueuedNoticeWithTheEstimate()
    {
        result = new HelpdeskActionResult(HelpdeskActionStatus.Queued, TimeOutcome: new HelpdeskTimeOutcome(HelpdeskTimeOutcome.BillablePending, 0m, 0.5m, 0m, IsEstimate: true));
        var vm = Create(remaining: null);
        vm.MinutesText = "30";

        await vm.SubmitAsync();

        Assert.Equal("Warn", vm.ResultState);
        Assert.StartsWith(loc["Helpdesk_Queued"], vm.ResultText, StringComparison.Ordinal);
        Assert.Contains(string.Format(loc.CurrentCulture, loc["Helpdesk_Time_Estimate"], string.Empty).Trim(), vm.ResultText, StringComparison.Ordinal);
        Assert.Equal(1, completed);
    }

    /// <summary>
    /// A refusal by the server and a local rejection are shown as errors, and the workbench is not refreshed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_RefusedOrRejected_ShowsTheError()
    {
        var vm = Create(remaining: null);
        vm.MinutesText = "30";

        result = new HelpdeskActionResult(HelpdeskActionStatus.Conflict, Detail: "The ticket is closed.");
        await vm.SubmitAsync();
        Assert.Equal("Bad", vm.ResultState);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["Helpdesk_Refused"], "The ticket is closed."), vm.ResultText);

        result = new HelpdeskActionResult(HelpdeskActionStatus.Rejected, ErrorKey: HelpdeskService.TextKey);
        await vm.SubmitAsync();
        Assert.Equal(loc[HelpdeskService.TextKey], vm.ResultText);
        Assert.Equal(0, completed);
    }

    /// <summary>
    /// A user without the permission sees the reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_WithoutPermission_ShowsTheReason()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<LogTicketTimeCommand>(), Any)).ThrowsAsync(new UnauthorizedAccessException("No access"));
        var vm = Create(remaining: null);
        vm.MinutesText = "30";

        await vm.SubmitAsync();

        Assert.Equal("No access", vm.ResultText);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// While typing, the dialog previews the settlement from the contract bank, and clears the preview for an invalid entry.
    /// </summary>
    [Fact]
    public void Preview_FollowsTheMinutesAndTheBillableFlag()
    {
        var vm = Create(remaining: 0.25m);
        var culture = loc.CurrentCulture;

        vm.MinutesText = "30";
        var expected = string.Format(
            culture,
            loc["Helpdesk_Time_Preview"],
            string.Format(culture, loc["Helpdesk_Time_Partial"], 0.25m.ToString("0.##", culture), 0.25m.ToString("0.##", culture)));
        Assert.Equal(expected, vm.PreviewText);

        vm.IsBillable = false;
        Assert.Equal(string.Format(culture, loc["Helpdesk_Time_Preview"], loc["Helpdesk_Time_NonBillable"]), vm.PreviewText);

        vm.MinutesText = "0";
        Assert.Equal(string.Empty, vm.PreviewText);
    }

    private HelpdeskTimeLogViewModel Create(decimal? remaining) =>
        new(
            sender.Object,
            loc,
            time,
            Ticket,
            remaining,
            () =>
            {
                completed++;

                return Task.CompletedTask;
            });
}
