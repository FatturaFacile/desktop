// <copyright file="CopilotApiClientTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using System.Net;
using System.Net.Http;
using System.Text;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Infrastructure.Copilot;
using FatturaFacile.Infrastructure.Services;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the typed HTTP client of the Copilot API against a fake server.
/// </summary>
public sealed class CopilotApiClientTests
{
    private static readonly Guid TenantId = new(0x11111111, 0x2222, 0x3333, 0x44, 0x44, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55);

    /// <summary>
    /// Every call carries the Bearer token and the tenant header; the streamed prompt injects the active context.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task StreamMessage_SendsHeadersContextAndParsesEvents()
    {
        const string sse = "event: message.started\ndata: {}\n\nevent: token\ndata: {\"text\":\"Ciao\"}\n\nevent: message.completed\ndata: {\"message_id\":\"m1\",\"conversation_id\":\"c1\"}\n\n";
        using FakeHandler handler = new(_ => Respond(HttpStatusCode.OK, sse, "text/event-stream"));
        using var client = CreateClient(handler, out var api);

        List<CopilotStreamEvent> events = [];

        await foreach (var item in api.StreamMessageAsync("c1", "Quanto ho fatturato?", "it-CH", new CopilotContext("/invoicing", "invoice", "42")))
        {
            events.Add(item);
        }

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://erp.example.com/api/v1/copilot/conversations/c1/messages", request.Uri);
        Assert.Equal("Bearer secret-token", request.Authorization);
        Assert.Equal(TenantId.ToString(), request.TenantHeader);
        Assert.Equal("text/event-stream", request.Accept);

        using var body = System.Text.Json.JsonDocument.Parse(request.Body);
        Assert.Equal("Quanto ho fatturato?", body.RootElement.GetProperty("prompt").GetString());
        Assert.Equal("it-CH", body.RootElement.GetProperty("locale").GetString());
        var context = body.RootElement.GetProperty("context");
        Assert.Equal("/invoicing", context.GetProperty("route").GetString());
        Assert.Equal("invoice", context.GetProperty("entity_type").GetString());
        Assert.Equal("42", context.GetProperty("entity_id").GetString());

        Assert.Equal([CopilotStreamEventKind.MessageStarted, CopilotStreamEventKind.Token, CopilotStreamEventKind.MessageCompleted], events.Select(static e => e.Kind));
    }

    /// <summary>
    /// The envelope is unwrapped and the conversation list is mapped.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task GetConversations_UnwrapsEnvelope()
    {
        using FakeHandler handler = new(_ => Respond(HttpStatusCode.OK, """{"success":true,"message":"ok","data":[{"id":"c1","title":"Fatture","locale":"it","context_route":null,"context_entity_type":null,"context_entity_id":null,"created_at":"2026-11-20T10:00:00Z","updated_at":null}],"meta":""}""", "application/json"));
        using var client = CreateClient(handler, out var api);

        var conversations = await api.GetConversationsAsync();

        var conversation = Assert.Single(conversations);
        Assert.Equal("c1", conversation.Id);
        Assert.Equal("Fatture", conversation.Title);
    }

    /// <summary>
    /// Confirm sends only the overrides; reject sends the reason.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task ConfirmAndReject_PostOverridesAndReason()
    {
        const string message = """{"success":true,"message":"ok","data":{"id":"m1","conversation_id":"c1","role":"assistant","content_markdown":"","citations":[],"visualization":null,"proposed_action":null,"action_status":"executed","token_usage":null,"created_at":null},"meta":""}""";
        using FakeHandler handler = new(_ => Respond(HttpStatusCode.OK, message, "application/json"));
        using var client = CreateClient(handler, out var api);

        var confirmed = await api.ConfirmActionAsync("m1", new Dictionary<string, object?> { ["items.0.quantity"] = 3m });
        _ = await api.RejectActionAsync("m1", "troppo costoso");

        Assert.Equal("executed", confirmed.ActionStatus);
        Assert.EndsWith("/actions/m1/confirm", handler.Requests[0].Uri, StringComparison.Ordinal);
        Assert.Contains("\"items.0.quantity\":3", handler.Requests[0].Body, StringComparison.Ordinal);
        Assert.EndsWith("/actions/m1/reject", handler.Requests[1].Uri, StringComparison.Ordinal);
        Assert.Contains("troppo costoso", handler.Requests[1].Body, StringComparison.Ordinal);
    }

    /// <summary>
    /// The documented error statuses are classified so a localized message can be shown.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected failure class.</param>
    /// <returns>A task representing the test.</returns>
    [Theory]
    [InlineData(403, CopilotApiErrorKind.Forbidden)]
    [InlineData(404, CopilotApiErrorKind.NotFound)]
    [InlineData(409, CopilotApiErrorKind.Conflict)]
    [InlineData(422, CopilotApiErrorKind.Unprocessable)]
    [InlineData(429, CopilotApiErrorKind.RateLimited)]
    [InlineData(502, CopilotApiErrorKind.ProviderUnavailable)]
    public async Task ErrorStatus_IsClassified(int status, CopilotApiErrorKind expected)
    {
        using FakeHandler handler = new(_ => Respond((HttpStatusCode)status, """{"success":false,"message":"server says no"}""", "application/json"));
        using var client = CreateClient(handler, out var api);

        var ex = await Assert.ThrowsAsync<CopilotApiException>(() => api.GetConversationAsync("c1"));

        Assert.Equal(expected, ex.Kind);
        Assert.Equal(status, ex.StatusCode);
    }

    /// <summary>
    /// A server that cannot be reached is reported as offline.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Unreachable_IsReportedAsOffline()
    {
        using FakeHandler handler = new(_ => throw new HttpRequestException("no route"));
        using var client = CreateClient(handler, out var api);

        var ex = await Assert.ThrowsAsync<CopilotApiException>(() => api.GetConversationsAsync());

        Assert.Equal(CopilotApiErrorKind.Offline, ex.Kind);
    }

    /// <summary>
    /// Without a configured address live calls are not possible.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task NotConfigured_ThrowsUnauthorized()
    {
        using HttpClient http = new();
        var api = new CopilotApiClient(http, new CopilotOptions(), CreateTenant());

        Assert.False(api.IsConfigured);
        var ex = await Assert.ThrowsAsync<CopilotApiException>(() => api.GetConversationsAsync());
        Assert.Equal(CopilotApiErrorKind.Unauthorized, ex.Kind);
    }

    private static CurrentTenantService CreateTenant()
    {
        CurrentTenantService tenant = new();
        tenant.SetCurrentTenant(TenantId, "ACME", "CH", "CHF");
        return tenant;
    }

    private static HttpResponseMessage Respond(HttpStatusCode status, string body, string mediaType) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) };

    private static HttpClient CreateClient(FakeHandler handler, out CopilotApiClient api)
    {
        HttpClient http = new(handler);
        api = new CopilotApiClient(http, new CopilotOptions { BaseUrl = "https://erp.example.com/api", AccessToken = "secret-token" }, CreateTenant());
        return http;
    }

    private sealed record RecordedRequest(string Uri, string? Authorization, string? TenantHeader, string? Accept, string Body);

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new RecordedRequest(
                request.RequestUri?.ToString() ?? string.Empty,
                request.Headers.Authorization?.ToString(),
                request.Headers.TryGetValues("X-Tenant-Id", out var values) ? values.FirstOrDefault() : null,
                request.Headers.Accept.FirstOrDefault()?.MediaType,
                body));
            return respond(request);
        }
    }
}
