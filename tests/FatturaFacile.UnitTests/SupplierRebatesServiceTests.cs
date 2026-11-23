// <copyright file="SupplierRebatesServiceTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the supplier rebate sync (cache refresh, offline fallback, classification of failures) and of the server actions (never
/// queued offline, cache refreshed once the server accepted them) with the handlers on top.
/// </summary>
public sealed class SupplierRebatesServiceTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTimeOffset Now = new(2027, 4, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<ISupplierRebatesApiClient> api = new();
    private readonly Mock<ISupplierRebatesCache> cache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierRebatesServiceTests"/> class.
    /// </summary>
    public SupplierRebatesServiceTests()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        _ = api.Setup(a => a.GetAgreementsAsync(Any)).ReturnsAsync("[" + SupplierRebatesTestData.AgreementJson + "]");
        _ = api.Setup(a => a.GetAgreementAsync(SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync(SupplierRebatesTestData.AgreementJson);
        _ = api.Setup(a => a.GetProgressAsync(SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync(SupplierRebatesTestData.ProgressJson);
        _ = api.Setup(a => a.GetRunsAsync(SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync(SupplierRebatesTestData.RunsJson);
        _ = api.Setup(a => a.GetProposalsAsync(SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync(SupplierRebatesTestData.ProposalsJson);
        _ = api.Setup(a => a.GetBlanketContractsAsync(Any)).ReturnsAsync(SupplierRebatesTestData.ContractsJson);
        _ = api.Setup(a => a.GetBlanketBalanceAsync(SupplierRebatesTestData.ContractId, Any)).ReturnsAsync(SupplierRebatesTestData.BalanceJson);
        _ = api.Setup(a => a.GetPassiveInvoicesAsync(Any)).ReturnsAsync(SupplierRebatesTestData.PassiveInvoicesJson);
    }

    /// <summary>
    /// Pulling an agreement stores its four payloads in one save, stamped with the clock.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncAgreement_Online_StoresTheFourPayloadsInOneSave()
    {
        var result = await Sync().SyncAgreementAsync(SupplierRebatesTestData.AgreementId, CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Synced, result.Status);
        Assert.Equal(Now.UtcDateTime, result.SyncedAtUtc);
        cache.Verify(
            c => c.SaveAsync(
                It.Is<IReadOnlyList<RebateCacheEntry>>(entries => entries.Count == 4
                    && entries.Any(e => e.Kind == RebateCacheKinds.Agreement && e.Key == SupplierRebatesTestData.AgreementId)
                    && entries.Any(e => e.Kind == RebateCacheKinds.Progress)
                    && entries.Any(e => e.Kind == RebateCacheKinds.Runs)
                    && entries.Any(e => e.Kind == RebateCacheKinds.Proposals)),
                Now.UtcDateTime,
                Any),
            Times.Once);
    }

    /// <summary>
    /// The list of the agreements and the blanket agreement data are cached under their own kind.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ListsAndBalance_AreCachedUnderTheirKind()
    {
        var sync = Sync();

        Assert.True((await sync.SyncAgreementsAsync(CancellationToken.None)).IsSuccess);
        Assert.True((await sync.SyncBlanketContractsAsync(CancellationToken.None)).IsSuccess);
        Assert.True((await sync.SyncBlanketBalanceAsync(SupplierRebatesTestData.ContractId, CancellationToken.None)).IsSuccess);

        cache.Verify(c => c.SaveAsync(It.Is<IReadOnlyList<RebateCacheEntry>>(e => e.Single().Kind == RebateCacheKinds.Agreements && e.Single().Key == RebateCacheKinds.AllKey), Now.UtcDateTime, Any), Times.Once);
        cache.Verify(c => c.SaveAsync(It.Is<IReadOnlyList<RebateCacheEntry>>(e => e.Single().Kind == RebateCacheKinds.BlanketContracts), Now.UtcDateTime, Any), Times.Once);
        cache.Verify(c => c.SaveAsync(It.Is<IReadOnlyList<RebateCacheEntry>>(e => e.Single().Kind == RebateCacheKinds.BlanketBalance && e.Single().Key == SupplierRebatesTestData.ContractId), Now.UtcDateTime, Any), Times.Once);
    }

    /// <summary>
    /// Without a session nothing is requested and the user is asked to sign in.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_WithoutSession_IsUnauthorizedAndAsksNothing()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Sync().SyncAgreementAsync(SupplierRebatesTestData.AgreementId, CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Unauthorized, result.Status);
        api.Verify(a => a.GetAgreementAsync(It.IsAny<string>(), Any), Times.Never);
        Assert.Equal(SupplierRebatesStatus.Unauthorized, (await Sync().LoadPayablesAsync(SupplierRebatesTestData.SupplierId, CancellationToken.None)).Status);
    }

    /// <summary>
    /// A server that cannot be reached, or a connection lost half way, leaves the cache untouched and reports offline; so does a timeout.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_ServerUnreachable_IsOfflineAndSavesNothing()
    {
        _ = api.Setup(a => a.GetRunsAsync(SupplierRebatesTestData.AgreementId, Any)).ThrowsAsync(new HttpRequestException("reset"));
        _ = api.Setup(a => a.GetAgreementsAsync(Any)).ThrowsAsync(new TaskCanceledException("timeout"));

        var detail = await Sync().SyncAgreementAsync(SupplierRebatesTestData.AgreementId, CancellationToken.None);
        var list = await Sync().SyncAgreementsAsync(CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Offline, detail.Status);
        Assert.Equal(SupplierRebatesStatus.Offline, list.Status);
        cache.Verify(c => c.SaveAsync(It.IsAny<IReadOnlyList<RebateCacheEntry>>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal is classified, and any other error is a failure.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected status.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, SupplierRebatesStatus.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, SupplierRebatesStatus.Forbidden)]
    [InlineData(HttpStatusCode.UnprocessableEntity, SupplierRebatesStatus.Rejected)]
    [InlineData(HttpStatusCode.Conflict, SupplierRebatesStatus.Rejected)]
    [InlineData(HttpStatusCode.InternalServerError, SupplierRebatesStatus.Failed)]
    public async Task Sync_HttpError_IsClassified(HttpStatusCode status, SupplierRebatesStatus expected)
    {
        _ = api.Setup(a => a.GetProgressAsync(SupplierRebatesTestData.AgreementId, Any)).ThrowsAsync(new HttpRequestException("refused", inner: null, status));

        var result = await Sync().SyncAgreementAsync(SupplierRebatesTestData.AgreementId, CancellationToken.None);

        Assert.Equal(expected, result.Status);
        Assert.Equal("refused", result.Detail);
        cache.Verify(c => c.SaveAsync(It.IsAny<IReadOnlyList<RebateCacheEntry>>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// A payload that does not parse never replaces a good cache.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_UnparsablePayload_FailsAndSavesNothing()
    {
        _ = api.Setup(a => a.GetProposalsAsync(SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync("not json");

        var result = await Sync().SyncAgreementAsync(SupplierRebatesTestData.AgreementId, CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Failed, result.Status);
        cache.Verify(c => c.SaveAsync(It.IsAny<IReadOnlyList<RebateCacheEntry>>(), It.IsAny<DateTime>(), Any), Times.Never);
    }

    /// <summary>
    /// The open payables are read live and filtered to the open invoices of the supplier; offline there are none.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task LoadPayables_ReadsLiveAndIsEmptyOffline()
    {
        var online = await Sync().LoadPayablesAsync(SupplierRebatesTestData.SupplierId, CancellationToken.None);
        _ = api.Setup(a => a.GetPassiveInvoicesAsync(Any)).ThrowsAsync(new HttpRequestException("down"));
        var offline = await Sync().LoadPayablesAsync(SupplierRebatesTestData.SupplierId, CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Synced, online.Status);
        Assert.Equal("p1", Assert.Single(online.Invoices).Id);
        Assert.Equal(SupplierRebatesStatus.Offline, offline.Status);
        Assert.Empty(offline.Invoices);
    }

    /// <summary>
    /// An accepted action returns the answer of the server and refreshes the cache of the list and of the agreement.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_Accepted_RefreshesTheCache()
    {
        _ = api.Setup(a => a.RunAccrualAsync(SupplierRebatesTestData.AgreementId, "{\"period_end\":\"2027-03-31\"}", Any)).ReturnsAsync("{\"id\":\"r2\"}");

        var result = await Writer().RunAccrualAsync(SupplierRebatesTestData.AgreementId, new DateOnly(2027, 3, 31), periodStart: null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("{\"id\":\"r2\"}", result.Data);
        api.Verify(a => a.GetAgreementsAsync(Any), Times.Once);
        api.Verify(a => a.GetAgreementAsync(SupplierRebatesTestData.AgreementId, Any), Times.Once);
        cache.Verify(c => c.SaveAsync(It.IsAny<IReadOnlyList<RebateCacheEntry>>(), Now.UtcDateTime, Any), Times.Exactly(2));
    }

    /// <summary>
    /// A new agreement has no identifier yet: the one in the answer of the server is refreshed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_CreateAgreement_RefreshesTheNewAgreement()
    {
        _ = api.Setup(a => a.CreateAgreementAsync(It.IsAny<string>(), Any)).ReturnsAsync(SupplierRebatesTestData.AgreementJson);

        var result = await Writer().SaveAgreementAsync(agreementId: null, NewAgreement(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        api.Verify(a => a.UpdateAgreementAsync(It.IsAny<string>(), It.IsAny<string>(), Any), Times.Never);
        api.Verify(a => a.GetAgreementAsync(SupplierRebatesTestData.AgreementId, Any), Times.Once);
    }

    /// <summary>
    /// Offline an action changes nothing, is not queued and says so; the cache is not touched.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_Offline_IsNotQueuedAndSavesNothing()
    {
        _ = api.Setup(a => a.AdvanceSettlementAsync(It.IsAny<string>(), It.IsAny<SettlementAction>(), It.IsAny<string>(), Any)).ThrowsAsync(new HttpRequestException("down"));

        var result = await Writer().AdvanceSettlementAsync(SupplierRebatesTestData.AgreementId, "s1", SettlementAction.Execute, new RebateSettlementStep(Version: 2), CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Offline, result.Status);
        Assert.False(result.IsSuccess);
        cache.Verify(c => c.SaveAsync(It.IsAny<IReadOnlyList<RebateCacheEntry>>(), It.IsAny<DateTime>(), Any), Times.Never);
        api.Verify(a => a.GetAgreementsAsync(Any), Times.Never);
    }

    /// <summary>
    /// A business refusal of the server (validation, state or version conflict) is reported with its message and refreshes nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_RefusedByTheServer_IsRejectedWithItsMessage()
    {
        _ = api.Setup(a => a.AdvanceSettlementAsync("s1", SettlementAction.Approve, "{\"version\":2}", Any))
            .ThrowsAsync(new HttpRequestException("The proposal was changed by someone else.", inner: null, HttpStatusCode.UnprocessableEntity));

        var result = await Writer().AdvanceSettlementAsync(SupplierRebatesTestData.AgreementId, "s1", SettlementAction.Approve, new RebateSettlementStep(Version: 2), CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Rejected, result.Status);
        Assert.Equal("The proposal was changed by someone else.", result.Detail);
        api.Verify(a => a.GetAgreementsAsync(Any), Times.Never);
    }

    /// <summary>
    /// Without a session an action is not sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Action_WithoutSession_IsUnauthorized()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);

        var result = await Writer().ChangeAgreementAsync(SupplierRebatesTestData.AgreementId, RebateAgreementAction.Activate, CancellationToken.None);

        Assert.Equal(SupplierRebatesStatus.Unauthorized, result.Status);
        api.Verify(a => a.ChangeAgreementStatusAsync(It.IsAny<string>(), It.IsAny<bool>(), Any), Times.Never);
    }

    /// <summary>
    /// Activating, closing and deleting an agreement call the matching endpoints; a delete refreshes only the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ChangeAgreement_CallsTheMatchingEndpoint()
    {
        _ = api.Setup(a => a.ChangeAgreementStatusAsync(It.IsAny<string>(), It.IsAny<bool>(), Any)).ReturnsAsync(SupplierRebatesTestData.AgreementJson);
        _ = api.Setup(a => a.DeleteAgreementAsync(It.IsAny<string>(), Any)).ReturnsAsync("{}");
        var writer = Writer();

        _ = await writer.ChangeAgreementAsync(SupplierRebatesTestData.AgreementId, RebateAgreementAction.Activate, CancellationToken.None);
        _ = await writer.ChangeAgreementAsync(SupplierRebatesTestData.AgreementId, RebateAgreementAction.Close, CancellationToken.None);
        api.Invocations.Clear();
        var deleted = await writer.ChangeAgreementAsync(SupplierRebatesTestData.AgreementId, RebateAgreementAction.Delete, CancellationToken.None);

        Assert.True(deleted.IsSuccess);
        api.Verify(a => a.DeleteAgreementAsync(SupplierRebatesTestData.AgreementId, Any), Times.Once);
        api.Verify(a => a.GetAgreementsAsync(Any), Times.Once);
        api.Verify(a => a.GetAgreementAsync(It.IsAny<string>(), Any), Times.Never);
    }

    /// <summary>
    /// The handlers read the cache without calling the server and hand the commands to the services.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Handlers_ReadTheCacheAndDelegateTheActions()
    {
        var syncedAt = new DateTime(2027, 4, 1, 8, 0, 0, DateTimeKind.Utc);
        _ = cache.Setup(c => c.FindAsync(RebateCacheKinds.Agreements, RebateCacheKinds.AllKey, Any)).ReturnsAsync(new StoredRebatePayload("[" + SupplierRebatesTestData.AgreementJson + "]", syncedAt));
        _ = cache.Setup(c => c.FindAsync(RebateCacheKinds.Agreement, SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync(new StoredRebatePayload(SupplierRebatesTestData.AgreementJson, syncedAt));
        _ = cache.Setup(c => c.FindAsync(RebateCacheKinds.Progress, SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync(new StoredRebatePayload(SupplierRebatesTestData.ProgressJson, syncedAt));
        _ = cache.Setup(c => c.FindAsync(RebateCacheKinds.Runs, SupplierRebatesTestData.AgreementId, Any)).ReturnsAsync(new StoredRebatePayload(SupplierRebatesTestData.RunsJson, syncedAt));
        _ = cache.Setup(c => c.FindAsync(RebateCacheKinds.BlanketBalance, SupplierRebatesTestData.ContractId, Any)).ReturnsAsync(new StoredRebatePayload(SupplierRebatesTestData.BalanceJson, syncedAt));

        var agreements = await new GetRebateAgreementsQueryHandler(cache.Object).Handle(new GetRebateAgreementsQuery(), CancellationToken.None);
        var detail = await new GetRebateAgreementDetailQueryHandler(cache.Object).Handle(new GetRebateAgreementDetailQuery(SupplierRebatesTestData.AgreementId), CancellationToken.None);
        var missing = await new GetRebateAgreementDetailQueryHandler(cache.Object).Handle(new GetRebateAgreementDetailQuery("other"), CancellationToken.None);
        var balance = await new GetBlanketBalanceQueryHandler(cache.Object).Handle(new GetBlanketBalanceQuery(SupplierRebatesTestData.ContractId), CancellationToken.None);
        var contracts = await new GetBlanketContractsQueryHandler(cache.Object).Handle(new GetBlanketContractsQuery(), CancellationToken.None);

        Assert.Equal(syncedAt, agreements.SyncedAtUtc);
        Assert.Single(agreements.Value);
        Assert.NotNull(detail);
        Assert.Equal(syncedAt, detail.SyncedAtUtc);
        Assert.NotNull(detail.Progress);
        Assert.Single(detail.Runs);
        Assert.Empty(detail.Proposals);
        Assert.Null(missing);
        Assert.Equal(85m, balance?.Value.CeilingUsedPercent);
        Assert.Null(contracts.SyncedAtUtc);
        Assert.Empty(contracts.Value);
        api.VerifyNoOtherCalls();

        var writer = new Mock<ISupplierRebatesWriteService>();
        _ = writer.Setup(w => w.RunAccrualAsync(SupplierRebatesTestData.AgreementId, null, null, Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Synced));
        _ = await new RunRebateAccrualCommandHandler(writer.Object).Handle(new RunRebateAccrualCommand(SupplierRebatesTestData.AgreementId), CancellationToken.None);
        writer.Verify(w => w.RunAccrualAsync(SupplierRebatesTestData.AgreementId, null, null, Any), Times.Once);
    }

    /// <summary>
    /// The sync handler pulls what the scope asks for, and a scope that needs an identifier refuses to run without one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncHandler_PullsTheScope()
    {
        var sync = new Mock<ISupplierRebatesSyncService>();
        _ = sync.Setup(s => s.SyncAgreementsAsync(Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Synced));
        _ = sync.Setup(s => s.SyncBlanketBalanceAsync("k", Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Offline));
        SyncSupplierRebatesCommandHandler handler = new(sync.Object);

        Assert.Equal(SupplierRebatesStatus.Synced, (await handler.Handle(new SyncSupplierRebatesCommand(RebateSyncScope.Agreements), CancellationToken.None)).Status);
        Assert.Equal(SupplierRebatesStatus.Offline, (await handler.Handle(new SyncSupplierRebatesCommand(RebateSyncScope.BlanketBalance, "k"), CancellationToken.None)).Status);
        _ = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(new SyncSupplierRebatesCommand(RebateSyncScope.Agreement), CancellationToken.None));
    }

    private static RebateAgreementInput NewAgreement() =>
        new(
            "PFA-2027",
            "Annual rebate",
            SupplierRebatesTestData.SupplierId,
            RebateTierMode.Retroactive,
            new DateOnly(2027, 1, 1),
            new DateOnly(2027, 12, 31),
            "CHF",
            RebateCodes.TypeTieredVolume,
            SupplierContractId: null,
            FixedBonusTarget: null,
            FixedBonusAmount: null,
            RebateCodes.FrequencyMonthly,
            AccrualAccountId: null,
            IncomeAccountId: null,
            Notes: null,
            SupplierRebatesTestData.Scale);

    private SupplierRebatesSyncService Sync() => new(api.Object, cache.Object, new FixedTime(Now));

    private SupplierRebatesWriteService Writer() => new(api.Object, Sync(), new FixedTime(Now));

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
