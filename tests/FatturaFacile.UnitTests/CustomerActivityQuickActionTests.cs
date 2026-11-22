// <copyright file="CustomerActivityQuickActionTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Application.Features.CustomerLifecycle;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.CustomerLifecycle;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the customer quick actions (log visit, create follow-up), of their offline outbox and of the replay.
/// </summary>
public sealed class CustomerActivityQuickActionTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Mock<ICustomerLifecycleApiClient> _api = new();
    private readonly List<CustomerActivityOutboxEntry> _stored = [];
    private readonly Mock<ICustomerActivityOutbox> _outbox = new();
    private readonly FixedTime _time = new(new DateTimeOffset(2026, 11, 22, 10, 0, 0, TimeSpan.Zero));

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerActivityQuickActionTests"/> class.
    /// </summary>
    public CustomerActivityQuickActionTests()
    {
        _ = _outbox.Setup(o => o.EnqueueAsync(It.IsAny<CustomerActivityOutboxEntry>(), Any))
            .Callback((CustomerActivityOutboxEntry entry, CancellationToken _) => _stored.Add(entry))
            .Returns(Task.CompletedTask);
        _ = _outbox.Setup(o => o.PendingAsync(Any)).ReturnsAsync(() => [.. _stored.Where(e => e.Status == CustomerActivityOutboxStatus.Pending)]);
        _ = _outbox.Setup(o => o.RemoveAsync(It.IsAny<CustomerActivityOutboxEntry>(), Any))
            .Callback((CustomerActivityOutboxEntry entry, CancellationToken _) => _stored.Remove(entry))
            .Returns(Task.CompletedTask);
        _ = _outbox.Setup(o => o.SaveAsync(Any)).Returns(Task.CompletedTask);
    }

    /// <summary>
    /// When the server answers, the activity is sent and nothing is queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_Online_SendsAndDoesNotQueue()
    {
        _ = _api.Setup(a => a.CreateActivityAsync(_accountId, CustomerActivityTypes.Meeting, "Visit", dueDate: null, Any)).Returns(Task.CompletedTask);

        var outcome = await Create().Handle(new CreateCustomerActivityCommand(_accountId, CustomerActivityTypes.Meeting, "  Visit  ", DueDate: null), CancellationToken.None);

        Assert.Equal(CustomerActivityOutcome.Sent, outcome);
        Assert.Empty(_stored);
    }

    /// <summary>
    /// A network failure (no status code) or a timeout saves the activity in the outbox.
    /// </summary>
    /// <param name="kind">The failure kind.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData("network")]
    [InlineData("timeout")]
    [InlineData("gateway")]
    public async Task Create_Unreachable_QueuesOffline(string kind)
    {
        Exception failure = kind switch
        {
            "network" => new HttpRequestException("no route"),
            "timeout" => new TaskCanceledException("timed out"),
            _ => new HttpRequestException("bad gateway", inner: null, HttpStatusCode.BadGateway),
        };
        _ = _api.Setup(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), Any)).ThrowsAsync(failure);
        var due = new DateTime(2026, 12, 1);

        var outcome = await Create().Handle(new CreateCustomerActivityCommand(_accountId, CustomerActivityTypes.FollowUp, "Call back", due), CancellationToken.None);

        Assert.Equal(CustomerActivityOutcome.QueuedOffline, outcome);
        var entry = Assert.Single(_stored);
        Assert.Equal(_accountId, entry.AccountId);
        Assert.Equal(CustomerActivityTypes.FollowUp, entry.Type);
        Assert.Equal("Call back", entry.Subject);
        Assert.Equal(due, entry.DueDate);
        Assert.Equal(CustomerActivityOutboxStatus.Pending, entry.Status);
    }

    /// <summary>
    /// A validation refusal of the server is raised to the caller and never queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_ValidationRefusal_IsNotQueued()
    {
        _ = _api.Setup(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), Any))
            .ThrowsAsync(new HttpRequestException("invalid", inner: null, HttpStatusCode.UnprocessableEntity));

        var failure = await Assert.ThrowsAsync<HttpRequestException>(() => Create().Handle(new CreateCustomerActivityCommand(_accountId, CustomerActivityTypes.Meeting, "Visit", DueDate: null), CancellationToken.None));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, failure.StatusCode);
        Assert.Empty(_stored);
    }

    /// <summary>
    /// A cancellation requested by the caller is not a network failure and is not queued.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Create_CallerCancellation_IsNotQueued()
    {
        _ = _api.Setup(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(() => Task.FromException(new TaskCanceledException()));

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Create().Handle(new CreateCustomerActivityCommand(_accountId, CustomerActivityTypes.Meeting, "Visit", DueDate: null), new CancellationToken(canceled: true)));

        Assert.Empty(_stored);
    }

    /// <summary>
    /// The subject is required and limited to 200 characters; the type must be a quick action type.
    /// </summary>
    /// <param name="type">The activity type.</param>
    /// <param name="length">The subject length.</param>
    /// <param name="valid">Whether the command is valid.</param>
    [Theory]
    [InlineData(CustomerActivityTypes.Meeting, 1, true)]
    [InlineData(CustomerActivityTypes.FollowUp, 200, true)]
    [InlineData(CustomerActivityTypes.FollowUp, 201, false)]
    [InlineData(CustomerActivityTypes.Meeting, 0, false)]
    [InlineData("call", 5, false)]
    public void Validator_EnforcesSubjectAndType(string type, int length, bool valid)
    {
        var result = new CreateCustomerActivityCommandValidator().Validate(new CreateCustomerActivityCommand(_accountId, type, new string('x', length), DueDate: null));

        Assert.Equal(valid, result.IsValid);
    }

    /// <summary>
    /// The replay sends the queued activities in order and removes them.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_Online_SendsInOrderAndRemoves()
    {
        _stored.Add(Entry("first"));
        _stored.Add(Entry("second"));
        List<string> sent = [];
        _ = _api.Setup(a => a.CreateActivityAsync(_accountId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), Any))
            .Callback((Guid _, string _, string subject, DateTime? _, CancellationToken _) => sent.Add(subject))
            .Returns(Task.CompletedTask);

        var count = await Flush().Handle(new FlushCustomerActivityOutboxCommand(), CancellationToken.None);

        Assert.Equal(2, count);
        Assert.Equal(["first", "second"], sent);
        Assert.Empty(_stored);
    }

    /// <summary>
    /// While the server is still unreachable the entry is kept, delayed, and the later ones are not tried.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_StillOffline_KeepsEntriesAndBacksOff()
    {
        var first = Entry("first");
        _stored.Add(first);
        _stored.Add(Entry("second"));
        _ = _api.Setup(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), Any)).ThrowsAsync(new HttpRequestException("down"));

        var count = await Flush().Handle(new FlushCustomerActivityOutboxCommand(), CancellationToken.None);

        Assert.Equal(0, count);
        Assert.Equal(2, _stored.Count);
        Assert.Equal(1, first.Attempts);
        Assert.True(first.NextAttemptAt > _time.GetUtcNow().UtcDateTime);
        _api.Verify(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), "second", It.IsAny<DateTime?>(), Any), Times.Never);

        // The next flush before the delay expires does not call the server again.
        _ = await Flush().Handle(new FlushCustomerActivityOutboxCommand(), CancellationToken.None);
        _api.Verify(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), Any), Times.Once);
    }

    /// <summary>
    /// An entry the server refuses becomes a conflict and the following entries are still sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Flush_RefusedEntry_BecomesConflictAndContinues()
    {
        var bad = Entry("bad");
        _stored.Add(bad);
        _stored.Add(Entry("good"));
        _ = _api.Setup(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), "bad", It.IsAny<DateTime?>(), Any)).ThrowsAsync(new HttpRequestException("invalid", inner: null, HttpStatusCode.UnprocessableEntity));
        _ = _api.Setup(a => a.CreateActivityAsync(It.IsAny<Guid>(), It.IsAny<string>(), "good", It.IsAny<DateTime?>(), Any)).Returns(Task.CompletedTask);

        var count = await Flush().Handle(new FlushCustomerActivityOutboxCommand(), CancellationToken.None);

        Assert.Equal(1, count);
        Assert.Equal(CustomerActivityOutboxStatus.Conflict, bad.Status);
        _ = Assert.Single(_stored);
    }

    /// <summary>
    /// The client posts the activity to crm/activities with the contract of the web frontend.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Client_CreateActivity_PostsContractBody()
    {
        string? body = null;
        Mock<ISyncApiClient> sync = new();
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Post, "crm/activities", It.IsAny<string?>(), Any))
            .Callback((HttpMethod _, string _, string? json, CancellationToken _) => body = json)
            .ReturnsAsync(new SyncApiReply(HttpStatusCode.Created, Encoding.UTF8.GetBytes("{}")));

        await new CustomerLifecycleApiClient(sync.Object).CreateActivityAsync(_accountId, CustomerActivityTypes.FollowUp, "Call back", new DateTime(2026, 12, 1), CancellationToken.None);

        Assert.NotNull(body);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        Assert.Equal("follow_up", root.GetProperty("type").GetString());
        Assert.Equal("Call back", root.GetProperty("subject").GetString());
        Assert.Equal("2026-12-01", root.GetProperty("due_date").GetString());
        Assert.Equal("account", root.GetProperty("related_type").GetString());
        Assert.Equal(_accountId.ToString("D"), root.GetProperty("related_id").GetString());
    }

    /// <summary>
    /// A refusal of the server carries its status code.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Client_CreateActivity_RefusalThrowsWithStatus()
    {
        Mock<ISyncApiClient> sync = new();
        _ = sync.Setup(s => s.SendAuthorizedAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), Any)).ReturnsAsync(new SyncApiReply(HttpStatusCode.UnprocessableEntity, Encoding.UTF8.GetBytes("{}")));

        var failure = await Assert.ThrowsAsync<HttpRequestException>(() => new CustomerLifecycleApiClient(sync.Object).CreateActivityAsync(_accountId, CustomerActivityTypes.Meeting, "Visit", dueDate: null, CancellationToken.None));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, failure.StatusCode);
    }

    /// <summary>
    /// The view model sends the follow-up of the shown customer and reports that it was saved offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ViewModel_FollowUp_ReportsOfflineQueue()
    {
        Mock<ISender> sender = new();
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerLifecycleQuery>(), Any)).ReturnsAsync(View(_accountId));
        _ = sender.Setup(s => s.Send(It.IsAny<CreateCustomerActivityCommand>(), Any)).ReturnsAsync(CustomerActivityOutcome.QueuedOffline);
        var viewModel = ViewModel(sender);
        await viewModel.ShowCommand.ExecuteAsync(parameter: null);
        viewModel.ActionSubject = "Call back";
        viewModel.ActionDueDate = new DateTime(2026, 12, 1);

        await viewModel.CreateFollowUpCommand.ExecuteAsync(parameter: null);

        Assert.Equal("[CustomerLifecycle_ActionQueued]", viewModel.StatusMessage);
        sender.Verify(s => s.Send(It.Is<CreateCustomerActivityCommand>(c => c.AccountId == _accountId && c.Type == CustomerActivityTypes.FollowUp && c.Subject == "Call back" && c.DueDate == new DateTime(2026, 12, 1)), Any), Times.Once);
    }

    /// <summary>
    /// An empty subject is refused locally and a server refusal shows the failure notice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ViewModel_LogVisit_ValidatesAndReportsRefusal()
    {
        Mock<ISender> sender = new();
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerLifecycleQuery>(), Any)).ReturnsAsync(View(_accountId));
        _ = sender.Setup(s => s.Send(It.IsAny<CreateCustomerActivityCommand>(), Any)).ThrowsAsync(new HttpRequestException("invalid", inner: null, HttpStatusCode.UnprocessableEntity));
        var viewModel = ViewModel(sender);
        await viewModel.ShowCommand.ExecuteAsync(parameter: null);

        await viewModel.LogVisitCommand.ExecuteAsync(parameter: null);
        Assert.Equal("[CustomerLifecycle_ActionInvalid]", viewModel.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<CreateCustomerActivityCommand>(), Any), Times.Never);

        viewModel.ActionSubject = "Visit";
        await viewModel.LogVisitCommand.ExecuteAsync(parameter: null);
        Assert.Equal("[CustomerLifecycle_ActionFailed]", viewModel.StatusMessage);
    }

    private static CustomerLifecycleView View(Guid accountId) => new(
        new CustomerLifecycleSummary(new CustomerLifecycleHealth(HealthIndex: 70, ChurnRisk: "low"), new CustomerLifecycleKpis(LifetimeValue: "0", OpenPipelineValue: "0", OpenOpportunities: 0, OverdueReceivableAmount: "0", OpenTickets: 0, SlaBreachRate: 0, MttrHours: null, ActiveContracts: 0, ContractCoverage: "none", ContractHoursRemaining: "0", ContractsExpiringSoon: 0, InterventionsScheduled: 0, InterventionsCompleted: 0)),
        [],
        accountId);

    private static CustomerLifecycleViewModel ViewModel(Mock<ISender> sender)
    {
        Mock<ILocalizationService> localization = new();
        _ = localization.Setup(l => l[It.IsAny<string>()]).Returns((string key) => "[" + key + "]");

        return new CustomerLifecycleViewModel(sender.Object, localization.Object)
        {
            SelectedCustomer = new CustomerChoice(Guid.NewGuid(), "Rossi SA"),
        };
    }

    private CustomerActivityOutboxEntry Entry(string subject) => new()
    {
        TenantId = Guid.NewGuid(),
        AccountId = _accountId,
        Type = CustomerActivityTypes.Meeting,
        Subject = subject,
        OccurredAtUtc = _time.GetUtcNow().UtcDateTime.AddMinutes(-_stored.Count - 1),
        NextAttemptAt = _time.GetUtcNow().UtcDateTime.AddMinutes(-1),
    };

    private CreateCustomerActivityCommandHandler Create()
    {
        Mock<ICurrentTenantService> tenant = new();
        _ = tenant.SetupGet(t => t.TenantId).Returns(Guid.NewGuid());

        return new CreateCustomerActivityCommandHandler(_api.Object, _outbox.Object, tenant.Object, _time);
    }

    private FlushCustomerActivityOutboxCommandHandler Flush() => new(_api.Object, _outbox.Object, _time);

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
