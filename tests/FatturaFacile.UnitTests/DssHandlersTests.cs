// <copyright file="DssHandlersTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Dss;
using FatturaFacile.Application.Features.Dss;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.Dss;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Executive DSS queries and commands and of the HTTP client behind them.
/// </summary>
public sealed class DssHandlersTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    /// <summary>
    /// The snapshot query reads the cache only: nothing cached means no snapshot.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetSnapshot_NothingCached_ReturnsNull()
    {
        Mock<IDssSnapshotStore> store = new();
        _ = store.Setup(s => s.FindAsync(It.IsAny<DssFilter>(), Any)).ReturnsAsync((StoredDssPayload?)null);

        var snapshot = await new GetDssSnapshotQueryHandler(store.Object).Handle(new GetDssSnapshotQuery(DssFilter.Default), CancellationToken.None);

        Assert.Null(snapshot);
    }

    /// <summary>
    /// A cached payload becomes a complete snapshot: read models, the offline baseline and the legal entities.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetSnapshot_Cached_ReturnsParsedSnapshot()
    {
        var at = new DateTime(2026, 6, 30, 10, 0, 0, DateTimeKind.Utc);
        Mock<IDssSnapshotStore> store = new();
        _ = store.Setup(s => s.FindAsync(DssFilter.Default, Any)).ReturnsAsync(new StoredDssPayload(DssTestData.Payload(), at));

        var snapshot = await new GetDssSnapshotQueryHandler(store.Object).Handle(new GetDssSnapshotQuery(DssFilter.Default), CancellationToken.None);

        Assert.NotNull(snapshot);
        Assert.Equal(at, snapshot.SyncedAtUtc);
        Assert.Equal(8, snapshot.Overview.Kpis.Count);
        Assert.Equal(2, snapshot.Profitability.Rows.Count);
        Assert.Equal(7, snapshot.VarianceBridge.EbitdaSteps.Count);
        Assert.Equal(1000, snapshot.Baseline.Revenue);
        Assert.Equal(2, snapshot.LegalEntities.Count);
    }

    /// <summary>
    /// The sync command hands the filter to the sync service and returns its outcome.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncCommand_DelegatesToTheSyncService()
    {
        Mock<IDssSyncService> sync = new();
        var expected = new DssSyncResult(DssSyncStatus.Offline, Detail: "down");
        _ = sync.Setup(s => s.SyncAsync(DssFilter.Default, Any)).ReturnsAsync(expected);

        var result = await new SyncDssSnapshotCommandHandler(sync.Object).Handle(new SyncDssSnapshotCommand(DssFilter.Default), CancellationToken.None);

        Assert.Same(expected, result);
    }

    /// <summary>
    /// The board pack comes from the server and gets a name from the filter.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ExportBoardPack_ReturnsTheServerFileWithAName()
    {
        Mock<IDssApiClient> api = new();
        byte[] content = [1, 2, 3];
        _ = api.Setup(a => a.ExportBoardPackAsync(DssFilter.Default, "pdf", "it-CH", Any)).ReturnsAsync(content);

        var pack = await new ExportDssBoardPackQueryHandler(api.Object).Handle(new ExportDssBoardPackQuery(DssFilter.Default, "pdf", "it-CH"), CancellationToken.None);

        Assert.Equal("board-pack-ytd-chf.pdf", pack.FileName);
        Assert.Equal(content, pack.Content.ToArray());
    }

    /// <summary>
    /// The client asks the overview with the whole filter in the query string, the legal entity in the Laravel array form.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApiClient_Overview_SendsTheFilter()
    {
        Mock<ISyncApiClient> sync = new();
        string? url = null;
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, It.IsAny<string>(), jsonBody: null, Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, address, _, _) => url = address)
            .ReturnsAsync(Reply("""{ "data": { "ok": true } }"""));

        var data = await new DssApiClient(sync.Object).GetOverviewAsync(new DssFilter("ent-1", "EUR", "QTD", "BUDGET"), CancellationToken.None);

        Assert.Equal("""{ "ok": true }""", data);
        Assert.Equal("analytics/dss/overview?currency=EUR&period_preset=QTD&comparison_baseline=BUDGET&legal_entity_ids%5B%5D=ent-1", url);
    }

    /// <summary>
    /// The neutral simulation used to fetch the what-if inputs posts the filter and six zero sliders.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApiClient_SimulationInputs_PostsANeutralScenario()
    {
        Mock<ISyncApiClient> sync = new();
        string? body = null;
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Post, "analytics/dss/simulate", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => body = json)
            .ReturnsAsync(Reply("""{ "data": { "inputs": {} } }"""));

        _ = await new DssApiClient(sync.Object).GetSimulationInputsAsync(DssFilter.Default, CancellationToken.None);

        using var document = JsonDocument.Parse(body!);
        var root = document.RootElement;
        Assert.Equal("CHF", root.GetProperty("currency").GetString());
        Assert.False(root.TryGetProperty("legal_entity_ids", out _));
        var scenario = root.GetProperty("scenario");
        foreach (var slider in new[] { "price_pct", "cogs_pct", "opex_pct", "dso_days_delta", "fx_shift_pct", "fte_delta" })
        {
            Assert.Equal(0, scenario.GetProperty(slider).GetInt32());
        }
    }

    /// <summary>
    /// An HTTP error carries its status, so the sync service can tell a refused session from an outage.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApiClient_HttpError_CarriesTheStatus()
    {
        Mock<ISyncApiClient> sync = new();
        _ = sync.Setup(s => s.SendAuthorizedAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), Any))
            .ReturnsAsync(new SyncApiReply(System.Net.HttpStatusCode.Forbidden, ReadOnlyMemory<byte>.Empty));

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => new DssApiClient(sync.Object).GetOverviewAsync(DssFilter.Default, CancellationToken.None));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, error.StatusCode);
    }

    /// <summary>
    /// An answer without a data object is not accepted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApiClient_AnswerWithoutData_Throws()
    {
        Mock<ISyncApiClient> sync = new();
        _ = sync.Setup(s => s.SendAuthorizedAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), Any)).ReturnsAsync(Reply("""{ "success": true }"""));

        _ = await Assert.ThrowsAsync<JsonException>(() => new DssApiClient(sync.Object).GetLegalEntitiesAsync(CancellationToken.None));
    }

    /// <summary>
    /// The board pack is posted with the format and language and comes back as bytes.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApiClient_ExportBoardPack_PostsFormatAndLocale()
    {
        Mock<ISyncApiClient> sync = new();
        string? body = null;
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Post, "analytics/dss/export-board-pack", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => body = json)
            .ReturnsAsync(new SyncApiReply(System.Net.HttpStatusCode.OK, "%PDF"u8.ToArray()));

        var bytes = await new DssApiClient(sync.Object).ExportBoardPackAsync(DssFilter.Default, "pdf", "de-CH", CancellationToken.None);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes));
        Assert.Contains("\"format\":\"pdf\"", body, StringComparison.Ordinal);
        Assert.Contains("\"locale\":\"de-CH\"", body, StringComparison.Ordinal);
    }

    private static SyncApiReply Reply(string json) => new(System.Net.HttpStatusCode.OK, Encoding.UTF8.GetBytes(json));
}
