// <copyright file="ManufacturingApiClientTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.Manufacturing;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the manufacturing HTTP client: paging, the OEE query and the failure mapping.
/// </summary>
public sealed class ManufacturingApiClientTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISyncApiClient> sync = new();

    /// <summary>
    /// A list is fetched page by page until the last page and the items are merged into one array.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetList_MergesEveryPage()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, "manufacturing/machines?per_page=100&page=1", null, Any))
            .ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": [ { "id": "a" }, { "id": "b" } ], "meta": { "last_page": 2 } }"""));
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, "manufacturing/machines?per_page=100&page=2", null, Any))
            .ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": [ { "id": "c" } ], "meta": { "last_page": 2 } }"""));
        var client = new ManufacturingApiClient(sync.Object);

        var json = await client.GetListAsync("machines", CancellationToken.None);

        Assert.Equal("""[{ "id": "a" },{ "id": "b" },{ "id": "c" }]""", json);
        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, It.IsAny<string>(), null, Any), Times.Exactly(2));
    }

    /// <summary>
    /// An answer without pagination metadata is a single page.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetList_WithoutMeta_IsOnePage()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, "manufacturing/routings?per_page=100&page=1", null, Any))
            .ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": [ { "id": "a" } ] }"""));
        var client = new ManufacturingApiClient(sync.Object);

        var json = await client.GetListAsync("routings", CancellationToken.None);

        Assert.Equal("""[{ "id": "a" }]""", json);
        sync.Verify(s => s.SendAuthorizedAsync(HttpMethod.Get, It.IsAny<string>(), null, Any), Times.Once);
    }

    /// <summary>
    /// A failing status surfaces as an HTTP exception that carries the status code.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetList_ForbiddenAnswer_ThrowsWithTheStatus()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, It.IsAny<string>(), null, Any)).ReturnsAsync(Reply(HttpStatusCode.Forbidden, "{}"));
        var client = new ManufacturingApiClient(sync.Object);

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetListAsync("work-centers", CancellationToken.None));

        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
    }

    /// <summary>
    /// An answer without a data list is rejected.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetList_WithoutData_Throws()
    {
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, It.IsAny<string>(), null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "message": "hi" }"""));
        var client = new ManufacturingApiClient(sync.Object);

        _ = await Assert.ThrowsAsync<JsonException>(() => client.GetListAsync("work-centers", CancellationToken.None));
    }

    /// <summary>
    /// The OEE summary is requested for the work centre and the period in UTC and the data object is returned.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetOeeSummary_SendsTheWorkCentreAndPeriod()
    {
        const string expectedUrl = "manufacturing/oee/summary?work_center_id=11111111-1111-1111-1111-111111111111&from_datetime=2026-10-22T08%3A00%3A00Z&to_datetime=2026-11-21T08%3A00%3A00Z";
        _ = sync.Setup(s => s.SendAuthorizedAsync(HttpMethod.Get, expectedUrl, null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": { "oee": 1 } }"""));
        var client = new ManufacturingApiClient(sync.Object);

        var json = await client.GetOeeSummaryAsync(
            ManufacturingTestData.WorkCenterA,
            new DateTime(2026, 10, 22, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 11, 21, 8, 0, 0, DateTimeKind.Utc),
            CancellationToken.None);

        Assert.Equal("""{ "oee": 1 }""", json);
    }

    /// <summary>
    /// The client reports the session state of the underlying API client.
    /// </summary>
    [Fact]
    public void IsAuthenticated_FollowsTheSession()
    {
        _ = sync.SetupGet(s => s.IsAuthenticated).Returns(value: true);

        Assert.True(new ManufacturingApiClient(sync.Object).IsAuthenticated);
    }

    private static SyncApiReply Reply(HttpStatusCode status, string body) => new(status, Encoding.UTF8.GetBytes(body));
}
