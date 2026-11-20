// <copyright file="SyncApiClientTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Text;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Sync;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the web-api HTTP client against a scripted handler.
/// </summary>
public sealed class SyncApiClientTests
{
    private const string LoginReply = """{"success":true,"data":{"token":"1|abc","active_tenant":{"id":"tenant-1"}}}""";
    private const string DeltaReply = """{"success":true,"data":{"data":[{"id":"0b0d1a3e-0000-4000-8000-000000000001","company_name":"A"}],"deleted":["0b0d1a3e-0000-4000-8000-000000000002"],"next_cursor":"c2","server_time":"2026-11-20T10:00:00+00:00","has_more":true}}""";

    /// <summary>
    /// Login stores the token and the active tenant.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Login_StoresTokenAndTenant()
    {
        using ScriptedHandler handler = new((_, _) => Json(HttpStatusCode.OK, LoginReply));
        var (client, session) = Create(handler);

        Assert.True(await client.LoginAsync("a@b.ch", "secret", CancellationToken.None));

        Assert.Equal("1|abc", session.Token);
        Assert.Equal("tenant-1", session.ServerTenantId);
        Assert.Equal("/api/v1/auth/login", handler.Requests[0].Path);
    }

    /// <summary>
    /// Wrong credentials leave the client signed out.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Login_Rejected_StaysSignedOut()
    {
        using ScriptedHandler handler = new((_, _) => Json(HttpStatusCode.UnprocessableEntity, "{}"));
        var (client, _) = Create(handler);

        Assert.False(await client.LoginAsync("a@b.ch", "wrong", CancellationToken.None));
        Assert.False(client.IsAuthenticated);
    }

    /// <summary>
    /// The delta request carries auth headers, the checkpoint and the cursor, and the page is parsed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task GetDelta_SendsHeadersAndParsesPage()
    {
        using ScriptedHandler handler = new((_, _) => Json(HttpStatusCode.OK, DeltaReply));
        var (client, session) = Create(handler);
        session.Set("tok", "tenant-1");

        var page = await client.GetDeltaAsync("contacts", new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc), "c1", 50, CancellationToken.None);

        var request = handler.Requests[0];
        Assert.Equal("/api/v1/mobile/sync/contacts", request.Path);
        Assert.Contains("limit=50", request.Query, StringComparison.Ordinal);
        Assert.Contains("cursor=c1", request.Query, StringComparison.Ordinal);
        Assert.Contains("updated_since=2026-11-20T09%3A00%3A00", request.Query, StringComparison.Ordinal);
        Assert.Equal("Bearer tok", request.Authorization);
        Assert.Equal("tenant-1", request.Tenant);
        _ = Assert.Single(page.Data);
        _ = Assert.Single(page.Deleted);
        Assert.Equal("c2", page.NextCursor);
        Assert.True(page.HasMore);
        Assert.Equal(new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc), page.ServerTime);
    }

    /// <summary>
    /// A write carries its idempotency key and a 401 refreshes the token once before it is replayed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Send_On401_RefreshesTokenAndReplaysWithSameKey()
    {
        var calls = 0;
        using ScriptedHandler handler = new((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("auth/refresh", StringComparison.Ordinal))
            {
                return Json(HttpStatusCode.OK, """{"data":{"token":"new-token"}}""");
            }

            calls++;
            return calls == 1 ? Json(HttpStatusCode.Unauthorized, "{}") : Json(HttpStatusCode.Created, "{}");
        });
        var (client, session) = Create(handler);
        session.Set("old-token", "tenant-1");
        var key = Guid.NewGuid();

        var outcome = await client.SendAsync(new SyncRequest(SyncOperation.Create, "contacts", Guid.NewGuid(), key, "{}"), CancellationToken.None);

        Assert.Equal(SyncSendResult.Success, outcome.Result);
        Assert.Equal("new-token", session.Token);
        var writes = handler.Requests.Where(r => r.Path.EndsWith("/contacts", StringComparison.Ordinal)).ToList();
        Assert.Equal(2, writes.Count);
        Assert.All(writes, w => Assert.Equal(key.ToString(), w.IdempotencyKey));
        Assert.Equal("Bearer new-token", writes[1].Authorization);
    }

    /// <summary>
    /// A failed refresh signs the user out and reports the write as unauthorized.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Send_On401_WithFailedRefresh_ClearsSession()
    {
        using ScriptedHandler handler = new((_, _) => Json(HttpStatusCode.Unauthorized, "{}"));
        var (client, session) = Create(handler);
        session.Set("old-token", "tenant-1");

        var outcome = await client.SendAsync(new SyncRequest(SyncOperation.Update, "contacts", Guid.NewGuid(), Guid.NewGuid(), "{}"), CancellationToken.None);

        Assert.Equal(SyncSendResult.Unauthorized, outcome.Result);
        Assert.False(client.IsAuthenticated);
    }

    /// <summary>
    /// HTTP statuses are classified into the outcomes the engine reacts to.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="expected">The expected outcome.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData(HttpStatusCode.OK, SyncOperation.Update, SyncSendResult.Success)]
    [InlineData(HttpStatusCode.NotFound, SyncOperation.Delete, SyncSendResult.Success)]
    [InlineData(HttpStatusCode.NotFound, SyncOperation.Update, SyncSendResult.Rejected)]
    [InlineData(HttpStatusCode.Conflict, SyncOperation.Update, SyncSendResult.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed, SyncOperation.Update, SyncSendResult.Conflict)]
    [InlineData(HttpStatusCode.UnprocessableEntity, SyncOperation.Create, SyncSendResult.Rejected)]
    [InlineData(HttpStatusCode.TooManyRequests, SyncOperation.Create, SyncSendResult.Transient)]
    [InlineData(HttpStatusCode.ServiceUnavailable, SyncOperation.Create, SyncSendResult.Transient)]
    public async Task Send_ClassifiesStatus(HttpStatusCode status, SyncOperation operation, SyncSendResult expected)
    {
        using ScriptedHandler handler = new((_, _) => Json(status, "{}"));
        var (client, session) = Create(handler);
        session.Set("tok", "tenant-1");

        var outcome = await client.SendAsync(new SyncRequest(operation, "contacts", Guid.NewGuid(), Guid.NewGuid(), "{}"), CancellationToken.None);

        Assert.Equal(expected, outcome.Result);
    }

    /// <summary>
    /// An offline network is a transient outcome, not an exception.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Send_NetworkDown_IsTransient()
    {
        using ScriptedHandler handler = new((_, _) => throw new HttpRequestException("no route"));
        var (client, session) = Create(handler);
        session.Set("tok", "tenant-1");

        var outcome = await client.SendAsync(new SyncRequest(SyncOperation.Create, "contacts", Guid.NewGuid(), Guid.NewGuid(), "{}"), CancellationToken.None);

        Assert.Equal(SyncSendResult.Transient, outcome.Result);
    }

    private static (SyncApiClient Client, SyncSession Session) Create(ScriptedHandler handler)
    {
        SyncSession session = new();
        return (new SyncApiClient(handler.Http, session), session);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body)
        => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private sealed record Recorded(string Path, string Query, string? Authorization, string? Tenant, string? IdempotencyKey);

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> script;

        public ScriptedHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> script)
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new Recorded(
                request.RequestUri!.AbsolutePath,
                request.RequestUri.Query,
                request.Headers.Authorization?.ToString(),
                request.Headers.TryGetValues("X-Tenant-Id", out var tenant) ? tenant.Single() : null,
                request.Headers.TryGetValues("Idempotency-Key", out var key) ? key.Single() : null));
            return Task.FromResult(script(request, cancellationToken));
        }
    }
}
