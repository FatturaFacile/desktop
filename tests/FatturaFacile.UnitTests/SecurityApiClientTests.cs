// <copyright file="SecurityApiClientTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Sync;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the offline audit batch and PII unmask calls of the web-api client.
/// </summary>
public sealed class SecurityApiClientTests
{
    private static readonly DateTime Occurred = new(2026, 11, 21, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The batch is posted to the offline-batches route with auth headers, the idempotency key and the offline origin metadata.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task UploadBatch_PostsSignedFieldsWithOfflineOrigin()
    {
        using RecordingHandler handler = new(_ => Json(HttpStatusCode.Created, """{"success":true,"data":{"received_at":"2026-11-21T12:00:00+00:00","entries":[{"sequence_number":2,"server_received_at":"2026-11-21T12:00:05+00:00","id":"srv-2"}]}}"""));
        var (client, session) = Create(handler);
        session.Set("tok", "tenant-1");
        var batchId = Guid.NewGuid();

        var outcome = await client.UploadAuditBatchAsync(new AuditBatchRequest(batchId, "device-1", [Entry(1), Entry(2)]), CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(SecurityApiRoutes.OfflineAuditBatches, request.Path);
        Assert.Equal("Bearer tok", request.Authorization);
        Assert.Equal("tenant-1", request.Tenant);
        Assert.Equal(batchId.ToString("D"), request.IdempotencyKey);

        using var body = JsonDocument.Parse(request.Body);
        var root = body.RootElement;
        Assert.Equal(SecurityApiRoutes.DesktopOfflineOrigin, root.GetProperty("origin").GetString());
        Assert.Equal("device-1", root.GetProperty("device_id").GetString());
        Assert.Equal(batchId.ToString("D"), root.GetProperty("batch_id").GetString());
        var entries = root.GetProperty("entries");
        Assert.Equal(2, entries.GetArrayLength());
        var first = entries[0];
        Assert.Equal(1, first.GetProperty("sequence_number").GetInt64());
        Assert.Equal(new string('0', 64), first.GetProperty("previous_hash").GetString());
        Assert.Equal(new string('b', 64), first.GetProperty("payload_hash").GetString());
        Assert.Equal(new string('c', 64), first.GetProperty("record_hash").GetString());
        Assert.Equal("2026-11-21T08:00:00.0000000Z", first.GetProperty("occurred_at").GetString());
        Assert.Equal("offline", first.GetProperty("origin").GetString());
        Assert.Equal(7, first.GetProperty("payload").GetProperty("v").GetInt32());

        Assert.Equal(SyncSendResult.Success, outcome.Result);
        Assert.Equal(new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc), outcome.ReceivedAt);
        var ack = Assert.Single(outcome.Entries!);
        Assert.Equal(2, ack.SequenceNumber);
        Assert.Equal(new DateTime(2026, 11, 21, 12, 0, 5, DateTimeKind.Utc), ack.ServerReceivedAt);
        Assert.Equal("srv-2", ack.ServerReference);
    }

    /// <summary>
    /// An accepted batch with an unreadable acknowledgement is still a success.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task UploadBatch_UnreadableAcknowledgement_StillSucceeds()
    {
        using RecordingHandler handler = new(_ => Json(HttpStatusCode.OK, "not json"));
        var (client, session) = Create(handler);
        session.Set("tok", "t");

        var outcome = await client.UploadAuditBatchAsync(new AuditBatchRequest(Guid.NewGuid(), "d", [Entry(1)]), CancellationToken.None);

        Assert.Equal(SyncSendResult.Success, outcome.Result);
        Assert.Null(outcome.ReceivedAt);
    }

    /// <summary>
    /// HTTP failures and network errors are classified, never thrown.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="expected">The expected classification.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.Conflict, SyncSendResult.Conflict)]
    [InlineData(HttpStatusCode.UnprocessableEntity, SyncSendResult.Rejected)]
    [InlineData(HttpStatusCode.ServiceUnavailable, SyncSendResult.Transient)]
    public async Task UploadBatch_FailureStatus_IsClassified(HttpStatusCode status, SyncSendResult expected)
    {
        using RecordingHandler handler = new(_ => Json(status, "{}"));
        var (client, session) = Create(handler);
        session.Set("tok", "t");

        var outcome = await client.UploadAuditBatchAsync(new AuditBatchRequest(Guid.NewGuid(), "d", [Entry(1)]), CancellationToken.None);

        Assert.Equal(expected, outcome.Result);
    }

    /// <summary>
    /// A network failure is transient.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task UploadBatch_NetworkFailure_IsTransient()
    {
        using RecordingHandler handler = new(_ => throw new HttpRequestException("offline"));
        var (client, session) = Create(handler);
        session.Set("tok", "t");

        var outcome = await client.UploadAuditBatchAsync(new AuditBatchRequest(Guid.NewGuid(), "d", [Entry(1)]), CancellationToken.None);

        Assert.Equal(SyncSendResult.Transient, outcome.Result);
    }

    /// <summary>
    /// The unmask request carries entity, field and justification and returns the revealed value.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Unmask_PostsRequestAndReturnsValue()
    {
        using RecordingHandler handler = new(_ => Json(HttpStatusCode.OK, """{"success":true,"data":{"value":"CH93 0076 2011 6238 5295 7"}}"""));
        var (client, session) = Create(handler);
        session.Set("tok", "tenant-1");

        var outcome = await client.UnmaskPiiAsync(new PiiUnmaskRequest("contacts", "c-1", "iban", "Customer asked for a refund"), CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(SecurityApiRoutes.PiiUnmask, request.Path);
        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal("contacts", body.RootElement.GetProperty("entity_type").GetString());
        Assert.Equal("c-1", body.RootElement.GetProperty("entity_id").GetString());
        Assert.Equal("iban", body.RootElement.GetProperty("field").GetString());
        Assert.Equal("Customer asked for a refund", body.RootElement.GetProperty("justification").GetString());
        Assert.Equal(SyncSendResult.Success, outcome.Result);
        Assert.Equal("CH93 0076 2011 6238 5295 7", outcome.Value);
    }

    /// <summary>
    /// A refused unmask request returns the error and no value.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Unmask_Forbidden_ReturnsNoValue()
    {
        using RecordingHandler handler = new(_ => Json(HttpStatusCode.Forbidden, """{"message":"no"}"""));
        var (client, session) = Create(handler);
        session.Set("tok", "t");

        var outcome = await client.UnmaskPiiAsync(new PiiUnmaskRequest("contacts", "c-1", "iban", "0123456789"), CancellationToken.None);

        Assert.Equal(SyncSendResult.Rejected, outcome.Result);
        Assert.Null(outcome.Value);
        Assert.NotNull(outcome.Error);
    }

    private static LocalAuditEntry Entry(long sequence) => new()
    {
        SequenceNumber = sequence,
        PreviousHash = new string('0', 64),
        PayloadHash = new string('b', 64),
        RecordHash = new string('c', 64),
        PayloadJson = """{"v":7}""",
        OccurredAt = Occurred,
        EntityType = "contacts",
        EntityId = "c-1",
        Action = "update",
        DeviceId = "device-1",
    };

    private static (SyncApiClient Client, SyncSession Session) Create(RecordingHandler handler)
    {
        SyncSession session = new();
        return (new SyncApiClient(handler.Http, session), session);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body)
        => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private sealed record Recorded(HttpMethod Method, string Path, string? Authorization, string? Tenant, string? IdempotencyKey, string Body);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> script;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> script)
        {
            this.script = script;
#pragma warning disable IDISP014 // One client per scripted handler, disposed together with it.
            Http = new HttpClient(this, disposeHandler: false) { BaseAddress = new Uri("https://api.test/") };
#pragma warning restore IDISP014
        }

        public HttpClient Http { get; }

        public List<Recorded> Requests { get; } = [];

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Http.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new Recorded(
                request.Method,
                request.RequestUri!.AbsolutePath,
                request.Headers.Authorization?.ToString(),
                request.Headers.TryGetValues("X-Tenant-Id", out var tenant) ? tenant.Single() : null,
                request.Headers.TryGetValues("Idempotency-Key", out var key) ? key.Single() : null,
                body));
            return script(request);
        }
    }
}
