// <copyright file="HelpdeskEscalationViewModelTests.cs" company="Fattura Facile">
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
/// Tests of the escalation actions of a ticket: the Field Service work order and the RMA, online, queued and refused.
/// </summary>
public sealed class HelpdeskEscalationViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly HelpdeskTicketRisk Ticket = new(Guid.NewGuid(), "TCK-7", "Printer", "high", "open", null, null, null, 90, []);

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly List<EscalateTicketToFieldServiceCommand> workOrders = [];
    private readonly List<EscalateTicketToRmaCommand> rmas = [];
    private HelpdeskActionResult result = new(HelpdeskActionStatus.Sent);
    private int completed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpdeskEscalationViewModelTests"/> class.
    /// </summary>
    public HelpdeskEscalationViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<EscalateTicketToFieldServiceCommand>(), Any))
            .Callback<IRequest<HelpdeskActionResult>, CancellationToken>((command, _) => workOrders.Add((EscalateTicketToFieldServiceCommand)command))
            .ReturnsAsync(() => result);
        _ = sender.Setup(s => s.Send(It.IsAny<EscalateTicketToRmaCommand>(), Any))
            .Callback<IRequest<HelpdeskActionResult>, CancellationToken>((command, _) => rmas.Add((EscalateTicketToRmaCommand)command))
            .ReturnsAsync(() => result);
    }

    /// <summary>
    /// The work order request carries the entered fields, with the planned dates converted to UTC, and the workbench is refreshed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task FieldService_SendsTheEnteredFields()
    {
        var vm = Create(["SN-1"]);
        var start = new DateTime(2027, 2, 1, 0, 0, 0, DateTimeKind.Unspecified);
        vm.WorkType = "repair";
        vm.ScheduledStart = start;
        vm.ScheduledEnd = start.AddDays(1);
        vm.Location = "Via Roma 1, Lugano";

        await vm.EscalateToFieldServiceAsync();

        var request = Assert.Single(workOrders).Request;
        Assert.Equal(Ticket.TicketId, request.TicketId);
        Assert.Equal("repair", request.WorkType);
        Assert.Equal(DateTime.SpecifyKind(start, DateTimeKind.Local).ToUniversalTime(), request.ScheduledStart);
        Assert.Equal(DateTimeKind.Utc, request.ScheduledStart?.Kind);
        Assert.Equal("Via Roma 1, Lugano", request.LocationAddress);
        Assert.Equal(loc["Helpdesk_FsDone"], vm.ResultText);
        Assert.Equal("Good", vm.ResultState);
        Assert.Equal(1, completed);
    }

    /// <summary>
    /// Without planned dates none are sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task FieldService_WithoutDates_SendsNone()
    {
        var vm = Create([]);

        await vm.EscalateToFieldServiceAsync();

        var request = Assert.Single(workOrders).Request;
        Assert.Null(request.ScheduledStart);
        Assert.Null(request.ScheduledEnd);
    }

    /// <summary>
    /// The RMA request carries the unit, the goodwill flag and the symptom, and the answer shows the tracking token.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Rma_SendsTheUnitAndShowsTheTrackingToken()
    {
        result = new HelpdeskActionResult(HelpdeskActionStatus.Sent, TrackingToken: "tok-9");
        var vm = Create(["SN-1", "SN-2"]);
        vm.SerialNumber = "SN-2";
        vm.Goodwill = true;
        vm.Symptom = "Does not boot";

        await vm.EscalateToRmaAsync();

        var request = Assert.Single(rmas).Request;
        Assert.Equal("SN-2", request.SerialNumber);
        Assert.True(request.Goodwill);
        Assert.Equal("Does not boot", request.ReportedSymptom);
        DssTestData.SameStrings(["SN-1", "SN-2"], vm.Serials);
        Assert.Contains("tok-9", vm.ResultText, StringComparison.Ordinal);
        Assert.StartsWith(loc["Helpdesk_RmaDone"], vm.ResultText, StringComparison.Ordinal);
    }

    /// <summary>
    /// Offline, the escalation is queued and the dialog says so; a refusal shows the reason; a rejection shows its key.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Escalation_QueuedRefusedOrRejected_ShowsTheOutcome()
    {
        var vm = Create([]);

        result = new HelpdeskActionResult(HelpdeskActionStatus.Queued);
        await vm.EscalateToRmaAsync();
        Assert.Equal("Warn", vm.ResultState);
        Assert.Equal(loc["Helpdesk_Queued"], vm.ResultText);
        Assert.Equal(1, completed);

        result = new HelpdeskActionResult(HelpdeskActionStatus.Conflict, Detail: "The RMA needs a unit.");
        await vm.EscalateToRmaAsync();
        Assert.Equal("Bad", vm.ResultState);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["Helpdesk_Refused"], "The RMA needs a unit."), vm.ResultText);

        result = new HelpdeskActionResult(HelpdeskActionStatus.Rejected, ErrorKey: HelpdeskService.ScheduleKey);
        await vm.EscalateToFieldServiceAsync();
        Assert.Equal(loc[HelpdeskService.ScheduleKey], vm.ResultText);
        Assert.Equal(1, completed);
    }

    /// <summary>
    /// A user without the permission sees the reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Escalation_WithoutPermission_ShowsTheReason()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<EscalateTicketToRmaCommand>(), Any)).ThrowsAsync(new UnauthorizedAccessException("No access"));
        var vm = Create([]);

        await vm.EscalateToRmaAsync();

        Assert.Equal("No access", vm.ResultText);
        Assert.False(vm.IsBusy);
    }

    private HelpdeskEscalationViewModel Create(IEnumerable<string> serials) =>
        new(
            sender.Object,
            loc,
            time,
            Ticket,
            serials,
            () =>
            {
                completed++;

                return Task.CompletedTask;
            });
}
