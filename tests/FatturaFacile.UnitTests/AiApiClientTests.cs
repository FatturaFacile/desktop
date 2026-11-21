// <copyright file="AiApiClientTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.AiCopilot;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the AI web-api client against a scripted transport.
/// </summary>
public sealed class AiApiClientTests
{
    private const string SuggestionId = "0b0d1a3e-0000-4000-8000-000000000001";

    private static readonly Guid SuggestionGuid = new(0x0b0d1a3e, 0x0000, 0x4000, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01);

    private readonly Mock<ISyncApiClient> transport = new();

    /// <summary>
    /// The suggestion list is parsed (JSON payloads kept raw, numbers and dates typed) and the page size is requested.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetSuggestions_ParsesEnvelope()
    {
        var json = $$$"""{"id":"{{{SuggestionId}}}","kind":"sales_quote","workflow":"quote","source_type":"email","source_reference":"mail-1","source_text":null,"proposed_payload":{"customer":"ACME"},"field_confidences":{"customer":0.9},"confidence":0.92,"amount":120.5,"reasoning":"clear","citations":[],"status":"pending","priority":"high","provider_used":"p","model_used":"m","expires_at":"2026-12-01T10:00:00+00:00","created_at":"2026-11-20T10:00:00Z"}""";
        var body = $$$"""{"success":true,"data":[{{{json}}}],"meta":{"current_page":1,"per_page":100,"total":7,"last_page":1}}""";
        Script(HttpMethod.Get, "ai/suggestions?per_page=100", HttpStatusCode.OK, body);

        var page = await Create().GetSuggestionsAsync(100, CancellationToken.None);

        Assert.Equal(7, page.Total);
        var item = Assert.Single(page.Items);
        Assert.Equal(SuggestionGuid, item.Id);
        Assert.Equal("""{"customer":"ACME"}""", item.ProposedPayloadJson);
        Assert.Equal(0.92, item.Confidence, 3);
        Assert.Equal(120.5m, item.Amount);
        Assert.Equal("high", item.Priority);
        Assert.Equal(new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc), item.CreatedAt);
    }

    /// <summary>
    /// A missing permission surfaces as an exception carrying the 403 status.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetSuggestions_Forbidden_ThrowsWithStatus()
    {
        Script(HttpMethod.Get, "ai/suggestions?per_page=100", HttpStatusCode.Forbidden, """{"title":"Forbidden","detail":"No ai_review module.","status":403}""");

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => Create().GetSuggestionsAsync(100, CancellationToken.None));

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Contains("ai_review", ex.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The insights are parsed with the cash flow bands, risk count, anomalies and recommendations.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetInsights_ParsesCashFlowAndLists()
    {
        var suggestion = $$$"""{"id":"{{{SuggestionId}}}","kind":"anomaly","confidence":0.5,"status":"pending","priority":"normal"}""";
        var body = $$$"""{"success":true,"data":{"cash_flow":{"currency":"CHF","dates":["2026-11-21","2026-11-22"],"p10":[1,2],"p50":[3,"4.5"],"p90":[5,6],"horizon_days":2},"payment_risk":[{"customer":"a"},{"customer":"b"}],"recommendations":[],"anomalies":[{{{suggestion}}}]}}""";
        Script(HttpMethod.Get, "ai/insights", HttpStatusCode.OK, body);

        var insights = await Create().GetInsightsAsync(CancellationToken.None);

        Assert.Equal("CHF", insights.CashFlow?.Currency);
        Assert.Equal(2, insights.CashFlow?.HorizonDays);
        Assert.Equal([3m, 4.5m], insights.CashFlow?.P50);
        Assert.Equal(2, insights.PaymentRiskCount);
        _ = Assert.Single(insights.Anomalies);
        Assert.Empty(insights.Recommendations);
    }

    /// <summary>
    /// A null cash flow (no forecast yet) is accepted.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetInsights_NullCashFlow_IsAccepted()
    {
        Script(HttpMethod.Get, "ai/insights", HttpStatusCode.OK, """{"success":true,"data":{"cash_flow":null,"payment_risk":[],"recommendations":[],"anomalies":[]}}""");

        var insights = await Create().GetInsightsAsync(CancellationToken.None);

        Assert.Null(insights.CashFlow);
    }

    /// <summary>
    /// Approve and reject post the documented bodies to the documented routes.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ApproveAndReject_PostDocumentedBodies()
    {
        string? approveBody = null;
        string? rejectBody = null;
        _ = transport
            .Setup(t => t.SendJsonAsync(HttpMethod.Post, $"ai/suggestions/{SuggestionId}/approve", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback((HttpMethod _, string _, string? body, CancellationToken _) => approveBody = body)
            .ReturnsAsync(new SyncHttpReply(HttpStatusCode.OK, "{}"));
        _ = transport
            .Setup(t => t.SendJsonAsync(HttpMethod.Post, $"ai/suggestions/{SuggestionId}/reject", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback((HttpMethod _, string _, string? body, CancellationToken _) => rejectBody = body)
            .ReturnsAsync(new SyncHttpReply(HttpStatusCode.OK, "{}"));
        var client = Create();
        var id = SuggestionGuid;

        var approved = await client.ApproveAsync(id, """{"amount":5}""", "good", CancellationToken.None);
        var rejected = await client.RejectAsync(id, "wrong customer", CancellationToken.None);

        Assert.Equal(AiApiStatus.Success, approved.Status);
        Assert.Equal(AiApiStatus.Success, rejected.Status);
        using var approve = JsonDocument.Parse(approveBody!);
        Assert.Equal(5, approve.RootElement.GetProperty("edited_payload").GetProperty("amount").GetInt32());
        Assert.Equal("good", approve.RootElement.GetProperty("feedback").GetString());
        using var reject = JsonDocument.Parse(rejectBody!);
        Assert.Equal("wrong customer", reject.RootElement.GetProperty("reason").GetString());
    }

    /// <summary>
    /// A missing edited payload and feedback are sent as null.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approve_WithoutEdits_SendsNulls()
    {
        string? sent = null;
        _ = transport
            .Setup(t => t.SendJsonAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback((HttpMethod _, string _, string? body, CancellationToken _) => sent = body)
            .ReturnsAsync(new SyncHttpReply(HttpStatusCode.OK, "{}"));

        _ = await Create().ApproveAsync(SuggestionGuid, editedPayloadJson: null, feedback: null, CancellationToken.None);

        using var document = JsonDocument.Parse(sent!);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("edited_payload").ValueKind);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("feedback").ValueKind);
    }

    /// <summary>
    /// The HTTP statuses of a decision are classified, with the problem detail of the server.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected classification.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(409, AiApiStatus.AlreadyDecided)]
    [InlineData(422, AiApiStatus.Incomplete)]
    [InlineData(404, AiApiStatus.NotFound)]
    [InlineData(401, AiApiStatus.Unauthorized)]
    [InlineData(503, AiApiStatus.Transient)]
    [InlineData(429, AiApiStatus.Transient)]
    [InlineData(403, AiApiStatus.Refused)]
    public async Task Decision_StatusIsClassified(int status, AiApiStatus expected)
    {
        _ = transport
            .Setup(t => t.SendJsonAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncHttpReply((HttpStatusCode)status, """{"title":"t","detail":"Because.","status":1}"""));

        var outcome = await Create().RejectAsync(SuggestionGuid, "reason", CancellationToken.None);

        Assert.Equal(expected, outcome.Status);
        Assert.Equal("Because.", outcome.Detail);
    }

    /// <summary>
    /// A network failure while deciding is a transient outcome, never an exception.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Decision_NetworkFailure_IsTransient()
    {
        _ = transport
            .Setup(t => t.SendJsonAsync(HttpMethod.Post, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("no route"));

        var outcome = await Create().ApproveAsync(SuggestionGuid, editedPayloadJson: null, feedback: null, CancellationToken.None);

        Assert.Equal(AiApiStatus.Transient, outcome.Status);
    }

    /// <summary>
    /// An edited payload that is not JSON is refused locally without a request.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Approve_InvalidEditedPayload_IsRefusedLocally()
    {
        var outcome = await Create().ApproveAsync(SuggestionGuid, "{not json", feedback: null, CancellationToken.None);

        Assert.Equal(AiApiStatus.Refused, outcome.Status);
        transport.Verify(t => t.SendJsonAsync(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private AiApiClient Create() => new(transport.Object);

    private void Script(HttpMethod method, string url, HttpStatusCode status, string body) =>
        transport
            .Setup(t => t.SendJsonAsync(method, url, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncHttpReply(status, body));
}
