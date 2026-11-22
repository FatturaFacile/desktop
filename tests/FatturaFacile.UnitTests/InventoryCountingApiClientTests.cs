// <copyright file="InventoryCountingApiClientTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.InventoryCounting;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.InventoryCounting;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the physical inventory HTTP client: the JSON to DTO mapping (decimals served as strings, nullable
/// percentages), the request bodies and the failure mapping.
/// </summary>
public sealed class InventoryCountingApiClientTests
{
    private const string VarianceJson = """{ "data": { "session": { "id": "s1", "session_number": "CNT-1", "status": "pending_approval", "count_type": "full_physical", "is_blind": true, "freezes_stock": true, "frozen_at": null, "approval_mode": "multi_level", "approval_level_required": 2 }, "summary": { "lines": 2, "counted_lines": 2, "uncounted_lines": 0, "differing_lines": 2, "recount_pending_lines": 0, "system_value_total": "1000.00", "gross_variance_value": "150.50", "net_variance_value": "-50.25", "approval_mode": "multi_level", "suggested_approval_level": 2, "within_tolerance": false }, "lines": [ { "id": "l1", "sku": "A1", "item_name": "Bolt", "counted_quantity": "8.0000", "is_counted": true, "recount_required": false, "recount_round": 0, "system_quantity": "10.0000", "unit_cost": "2.500000", "system_value": "25.00", "first_counted_quantity": "8.0000", "final_quantity": "8.0000", "variance_quantity": "-2.0000", "variance_value": "-5.00", "variance_percentage": "-20.00", "variance_percentage_undefined": false }, { "id": "l2", "sku": "B2", "item_name": "Nut", "counted_quantity": 3, "is_counted": true, "recount_required": true, "recount_round": 1, "system_quantity": "0.0000", "unit_cost": "1.000000", "system_value": "0.00", "final_quantity": "3.0000", "variance_quantity": "3.0000", "variance_value": "3.00", "variance_percentage": null, "variance_percentage_undefined": true } ], "approvals": [ { "id": "a1", "level": 1, "required_role": "warehouse_manager", "status": "approved", "decided_by": "u1", "decided_at": "2026-11-22T10:00:00Z", "notes": "ok" }, { "id": "a2", "level": 2, "required_role": "cfo", "status": "pending" } ], "adjustment": null } }""";

    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISyncApiClient> sync = new();

    /// <summary>
    /// The variance report maps decimals served as strings and numbers, the nullable percentage and the approvals.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetVariance_MapsLinesSummaryAndApprovals()
    {
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Get, "inventory/counts/s1/variance", null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, VarianceJson));
        var client = new InventoryCountingApiClient(sync.Object);

        var result = await client.GetVarianceAsync("s1", CancellationToken.None);

        Assert.True(result.IsSuccess);
        var report = result.Value!;
        Assert.Equal("pending_approval", report.Session.Status);
        Assert.Equal(2, report.Session.ApprovalLevelRequired);
        Assert.Equal(-50.25m, report.Summary.NetVarianceValue);
        Assert.Equal(150.50m, report.Summary.GrossVarianceValue);
        Assert.Equal(2, report.Summary.SuggestedApprovalLevel);
        Assert.False(report.Summary.WithinTolerance);
        Assert.Equal(-20.00m, report.Lines[0].VariancePercentage);
        Assert.False(report.Lines[0].VariancePercentageUndefined);
        Assert.Null(report.Lines[1].VariancePercentage);
        Assert.True(report.Lines[1].VariancePercentageUndefined);
        Assert.Equal(3m, report.Lines[1].CountedQuantity);
        Assert.Equal("approved", report.Approvals[0].Status);
        Assert.Equal(2, report.Approvals[1].Level);
        Assert.Null(report.Adjustment);
    }

    /// <summary>
    /// A paginated list is unwrapped from its data envelope.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetSessions_UnwrapsPaginatedList()
    {
        const string body = """{ "data": [ { "id": "s1", "session_number": "CNT-1", "status": "in_progress", "count_type": "spot_check", "is_blind": false, "freezes_stock": false, "frozen_at": "2026-11-22T08:00:00Z" } ], "meta": { "total": 1 } }""";
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Get, "inventory/counts?per_page=100", null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, body));
        var client = new InventoryCountingApiClient(sync.Object);

        var result = await client.GetSessionsAsync(CancellationToken.None);

        var session = Assert.Single(result.Value!);
        Assert.Equal("CNT-1", session.SessionNumber);
        Assert.Equal("2026-11-22T08:00:00Z", session.FrozenAt);
        Assert.False(session.IsBlind);
    }

    /// <summary>
    /// An ABC run maps the ranked items with their cumulative percentage and previous class.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RunAbc_MapsRankedItems_AndSendsOnlyTheGivenParameters()
    {
        string? sent = null;
        const string body = """{ "data": { "id": "r1", "period_days": 90, "a_share": "80.00", "b_share": "95.00", "total_consumption_value": "1234.50", "item_count": 2, "class_a_count": 1, "class_b_count": 0, "class_c_count": 1, "applied": true, "items": [ { "stock_item_id": "i1", "sku": "S1", "name": "One", "rank": 1, "consumption_value": "1000.00", "cumulative_share": "81.00", "abc_class": "A", "previous_class": "B", "is_manual_override": false, "is_new_item": false }, { "stock_item_id": "i2", "sku": "S2", "name": "Two", "rank": 2, "consumption_value": "234.50", "cumulative_share": "100.00", "abc_class": "C", "previous_class": null, "is_manual_override": true, "is_new_item": true } ] } }""";
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Post, "inventory/abc-runs", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => sent = json)
            .ReturnsAsync(Reply(HttpStatusCode.Created, body));
        var client = new InventoryCountingApiClient(sync.Object);

        var result = await client.RunAbcAsync(new AbcRunRequest { PeriodDays = 90, Apply = true }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(81.00m, result.Value!.Items[0].CumulativeShare);
        Assert.Equal("B", result.Value.Items[0].PreviousClass);
        Assert.Null(result.Value.Items[1].PreviousClass);
        Assert.True(result.Value.Items[1].IsManualOverride);

        using var document = JsonDocument.Parse(sent!);
        Assert.Equal(90, document.RootElement.GetProperty("period_days").GetInt32());
        Assert.True(document.RootElement.GetProperty("apply").GetBoolean());
        Assert.False(document.RootElement.TryGetProperty("a_share", out _));
        Assert.False(document.RootElement.TryGetProperty("warehouse_id", out _));
    }

    /// <summary>
    /// Clearing an override sends an explicit null class together with the reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task OverrideAbcClass_Clear_SendsExplicitNull()
    {
        string? sent = null;
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Post, "inventory/abc-overrides", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => sent = json)
            .ReturnsAsync(Reply(HttpStatusCode.Created, """{ "success": true }"""));
        var client = new InventoryCountingApiClient(sync.Object);

        var result = await client.OverrideAbcClassAsync("i1", abcClass: null, "back to automatic", CancellationToken.None);

        Assert.True(result.IsSuccess);
        using var document = JsonDocument.Parse(sent!);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("abc_class").ValueKind);
        Assert.Equal("i1", document.RootElement.GetProperty("stock_item_id").GetString());
        Assert.Equal("back to automatic", document.RootElement.GetProperty("reason").GetString());
    }

    /// <summary>
    /// A new session is serialized with snake case names and without the unset optional fields.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task CreateSession_SerializesRequest()
    {
        string? sent = null;
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Post, "inventory/counts", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => sent = json)
            .ReturnsAsync(Reply(HttpStatusCode.Created, """{ "data": { "id": "s9", "session_number": "CNT-9", "status": "in_progress", "count_type": "zone_sweep" } }"""));
        var client = new InventoryCountingApiClient(sync.Object);

        var result = await client.CreateSessionAsync(
            new CreateCountSessionRequest { WarehouseId = "w1", CountType = "zone_sweep", ScheduledDate = "2026-12-01", IsBlind = true, BinIds = ["b1", "b2"] },
            CancellationToken.None);

        Assert.Equal("CNT-9", result.Value!.SessionNumber);
        using var document = JsonDocument.Parse(sent!);
        Assert.Equal("w1", document.RootElement.GetProperty("warehouse_id").GetString());
        Assert.Equal("zone_sweep", document.RootElement.GetProperty("count_type").GetString());
        Assert.Equal(2, document.RootElement.GetProperty("bin_ids").GetArrayLength());
        Assert.False(document.RootElement.TryGetProperty("notes", out _));
    }

    /// <summary>
    /// Approve sends the level in the URL and the notes only when given; reject sends the reason.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApproveAndReject_UseTheLevelRoutes()
    {
        string? approveBody = null;
        string? rejectBody = null;
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Post, "inventory/counts/s1/approvals/1/approve", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => approveBody = json)
            .ReturnsAsync(Reply(HttpStatusCode.OK, "{}"));
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Post, "inventory/counts/s1/approvals/2/reject", It.IsAny<string?>(), Any))
            .Callback<HttpMethod, string, string?, CancellationToken>((_, _, json, _) => rejectBody = json)
            .ReturnsAsync(Reply(HttpStatusCode.OK, string.Empty));
        var client = new InventoryCountingApiClient(sync.Object);

        var approved = await client.ApproveAsync("s1", 1, notes: null, CancellationToken.None);
        var rejected = await client.RejectAsync("s1", 2, "wrong bin", CancellationToken.None);

        Assert.True(approved.IsSuccess);
        Assert.True(rejected.IsSuccess);
        Assert.Equal("{}", approveBody);
        using var document = JsonDocument.Parse(rejectBody!);
        Assert.Equal("wrong bin", document.RootElement.GetProperty("reason").GetString());
    }

    /// <summary>
    /// The run-due answer carries the number of opened counts.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task RunDue_ReadsTheOpenedCount()
    {
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Post, "inventory/cycle-count-plans/run-due", null, Any)).ReturnsAsync(Reply(HttpStatusCode.OK, """{ "data": { "opened": 3 } }"""));
        var client = new InventoryCountingApiClient(sync.Object);

        var result = await client.RunDuePlansAsync(CancellationToken.None);

        Assert.Equal(3, result.Value);
    }

    /// <summary>
    /// A 422 answer is returned with the flattened validation messages, a 403 is flagged as forbidden.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Failures_AreReturnedNotThrown()
    {
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Post, "inventory/counts/s1/unfreeze", It.IsAny<string?>(), Any))
            .ReturnsAsync(Reply(HttpStatusCode.UnprocessableEntity, """{ "message": "The given data was invalid.", "errors": { "reason": [ "The reason field is required." ] } }"""));
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Get, "inventory/counts/s1/variance", null, Any)).ReturnsAsync(Reply(HttpStatusCode.Forbidden, """{ "message": "This action is unauthorized." }"""));
        var client = new InventoryCountingApiClient(sync.Object);

        var invalid = await client.UnfreezeAsync("s1", string.Empty, CancellationToken.None);
        var forbidden = await client.GetVarianceAsync("s1", CancellationToken.None);

        Assert.False(invalid.IsSuccess);
        Assert.True(invalid.IsValidationError);
        Assert.Equal("The reason field is required.", Assert.Single(invalid.Errors));
        Assert.Equal("The given data was invalid.", invalid.Message);
        Assert.True(forbidden.IsForbidden);
    }

    /// <summary>
    /// A network failure is reported as offline instead of throwing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task NetworkFailure_IsOffline()
    {
        _ = sync.Setup(s => s.SendJsonAsync(HttpMethod.Get, "inventory/cycle-count-plans", null, Any)).ThrowsAsync(new HttpRequestException("no route"));
        var client = new InventoryCountingApiClient(sync.Object);

        var result = await client.GetPlansAsync(CancellationToken.None);

        Assert.True(result.IsOffline);
        Assert.Equal("no route", result.Message);
    }

    private static SyncHttpReply Reply(HttpStatusCode status, string body) => new(status, body);
}
