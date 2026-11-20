// <copyright file="IamAuthorizationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Common.Security.Iam;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests for the offline IAM enforcement of the desktop client: deny-by-default authorization, field masking, the
/// encrypted permission cache and the revocation handling of the API session.
/// </summary>
public sealed class IamAuthorizationTests : IDisposable
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid UserA = Guid.NewGuid();
    private static readonly Guid UserB = Guid.NewGuid();

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"iam-cache-{Guid.NewGuid():N}.db");

    private static Guid? NoCompany => default;

    /// <inheritdoc/>
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    /// <summary>
    /// Verifies that nothing is granted without a snapshot and that wildcards and exact keys grant.
    /// </summary>
    [Fact]
    public void Authorization_DeniesByDefaultAndHonoursWildcards()
    {
        var authorization = new AuthorizationService();
        Assert.False(authorization.IsGranted("crm.contacts.view"));

        authorization.Apply(Snapshot(TenantA, UserA, NoCompany, ["crm.contacts.view", "ledger.*"]));

        Assert.True(authorization.IsGranted("crm.contacts.view"));
        Assert.False(authorization.IsGranted("crm.contacts.delete"));
        Assert.True(authorization.IsGranted("ledger.journals.post"));
        Assert.False(authorization.IsGranted("hr.payroll.view"));

        authorization.Apply(Snapshot(TenantA, UserA, NoCompany, ["*"]));
        Assert.True(authorization.IsGranted("anything.at.all"));

        authorization.Apply(snapshot: null);
        Assert.False(authorization.IsGranted("anything.at.all"));
    }

    /// <summary>
    /// Verifies that the most restrictive visibility of a field wins.
    /// </summary>
    [Fact]
    public void Authorization_AppliesTheMostRestrictiveFieldVisibility()
    {
        var authorization = new AuthorizationService();
        authorization.Apply(new PermissionSnapshot(
            TenantA,
            UserA,
            CompanyId: null,
            [],
            [
                new FieldPolicyEntry("hr.employee", "salary", FieldVisibility.Masked, "amount"),
                new FieldPolicyEntry("hr.employee", "salary", FieldVisibility.Hidden, "amount"),
                new FieldPolicyEntry("hr.employee", "iban", FieldVisibility.Masked, "iban"),
            ],
            DateTimeOffset.UtcNow));

        Assert.Equal(FieldVisibility.Hidden, authorization.VisibilityOf("hr.employee", "salary"));
        Assert.Equal(FieldVisibility.Masked, authorization.VisibilityOf("hr.employee", "iban"));
        Assert.Equal(FieldVisibility.Visible, authorization.VisibilityOf("hr.employee", "email"));
    }

    /// <summary>
    /// Verifies the masks of IBANs and amounts and the omission of hidden fields.
    /// </summary>
    [Fact]
    public void FieldMasker_MasksIbanAmountAndHidesHiddenFields()
    {
        Assert.Equal("CH** **** **** **** *295 7", FieldMasker.MaskIban("CH93 0076 2011 6238 5295 7"));
        Assert.EndsWith("3456", FieldMasker.MaskIban("IT60X0542811101000000123456").Replace(" ", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Equal(FieldMasker.AmountMask, FieldMasker.MaskIban("x"));

        Assert.Equal(FieldMasker.AmountMask, FieldMasker.Render(83250.5m, FieldVisibility.Masked, "amount"));
        Assert.Null(FieldMasker.Render(83250.5m, FieldVisibility.Hidden, "amount"));
        Assert.Equal("CH** **** **** **** *295 7", FieldMasker.Render("CH9300762011623852957", FieldVisibility.Masked, FieldMasker.IbanKind));
        Assert.DoesNotContain("83250", FieldMasker.Render(83250.5m, FieldVisibility.Masked, "amount"), StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that snapshots are isolated per tenant, user and company, and encrypted at rest.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PermissionCache_IsolatesTenantUserAndCompanyAndEncryptsAtRest()
    {
        var cache = NewCache();
        var company = Guid.NewGuid();
        await cache.SaveAsync(Snapshot(TenantA, UserA, NoCompany, ["secret.reports.view"]));
        await cache.SaveAsync(Snapshot(TenantA, UserA, company, ["company.only.view"]));

        Assert.Equal(["secret.reports.view"], (await cache.LoadAsync(TenantA, UserA, NoCompany))!.Permissions);
        Assert.Equal(["company.only.view"], (await cache.LoadAsync(TenantA, UserA, company))!.Permissions);
        Assert.Null(await cache.LoadAsync(TenantB, UserA, NoCompany));
        Assert.Null(await cache.LoadAsync(TenantA, UserB, NoCompany));
        Assert.Null(await cache.LoadAsync(TenantA, UserA, Guid.NewGuid()));

        await using var connection = new SqliteConnection($"Data Source={_databasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT payload FROM iam_permission_snapshots";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var raw = reader.GetString(0);
            Assert.StartsWith(AesGcmFieldEncryptor.Prefix, raw, StringComparison.Ordinal);
            Assert.DoesNotContain("secret.reports", raw, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Verifies that a revoked user gets no cached rights back and that a new snapshot restores them.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task PermissionCache_NeverReturnsRevokedRights()
    {
        var cache = NewCache();
        await cache.SaveAsync(Snapshot(TenantA, UserA, NoCompany, ["crm.contacts.view"]));
        await cache.SaveAsync(Snapshot(TenantA, UserB, NoCompany, ["crm.contacts.view"]));

        await cache.RevokeUserAsync(UserA);

        Assert.Null(await cache.LoadAsync(TenantA, UserA, NoCompany));
        Assert.NotNull(await cache.LoadAsync(TenantA, UserB, NoCompany));

        await cache.SaveAsync(Snapshot(TenantA, UserA, NoCompany, ["crm.contacts.view"]));
        Assert.NotNull(await cache.LoadAsync(TenantA, UserA, NoCompany));

        await cache.PurgeAsync();
        Assert.Null(await cache.LoadAsync(TenantA, UserB, NoCompany));
    }

    /// <summary>
    /// Verifies that a 401 purges the token, the snapshot in force and the cached rights.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ApiSessionHandler_PurgesEverythingOnUnauthorized()
    {
        var (session, authorization, cache, raised) = NewSession();
        session.Start("token-1");
        authorization.Apply(Snapshot(TenantA, UserA, NoCompany, ["crm.contacts.view"]));
        await cache.SaveAsync(Snapshot(TenantA, UserA, NoCompany, ["crm.contacts.view"]));

        using var response = await Send(session, HttpStatusCode.Unauthorized, "{}", "https://api.test/api/v1/invoices");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(session.BearerToken);
        Assert.False(authorization.IsGranted("crm.contacts.view"));
        Assert.Null(await cache.LoadAsync(TenantA, UserA, NoCompany));
        Assert.Equal([ApiProblemKind.SessionRevoked], raised);
    }

    /// <summary>
    /// Verifies that a 401 of the login endpoint itself is a wrong password, not a revoked session.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ApiSessionHandler_IgnoresUnauthorizedOfTheLoginEndpoint()
    {
        var (session, _, _, raised) = NewSession();
        session.Start("token-1");

        using var response = await Send(session, HttpStatusCode.Unauthorized, "{}", "https://api.test/api/v1/auth/login");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("token-1", session.BearerToken);
        Assert.Empty(raised);
    }

    /// <summary>
    /// Verifies the handling of the three structured 403 codes and that the body stays readable.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ApiSessionHandler_HandlesTheStructured403Codes()
    {
        var (session, authorization, cache, raised) = NewSession();
        session.Start("token-1");
        authorization.Apply(Snapshot(TenantA, UserA, NoCompany, ["crm.contacts.view"]));
        await cache.SaveAsync(Snapshot(TenantA, UserA, NoCompany, ["crm.contacts.view"]));

        using var denied = await Send(session, HttpStatusCode.Forbidden, "{\"code\":\"permission_denied\"}", "https://api.test/api/v1/x");
        Assert.Contains("permission_denied", await denied.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        using var mfa = await Send(session, HttpStatusCode.Forbidden, "{\"code\":\"mfa_required\"}", "https://api.test/api/v1/x");
        using var unknown = await Send(session, HttpStatusCode.Forbidden, "not json", "https://api.test/api/v1/x");

        Assert.Equal([ApiProblemKind.PermissionDenied, ApiProblemKind.MfaRequired], raised);
        Assert.Equal("token-1", session.BearerToken);

        using var revoked = await Send(session, HttpStatusCode.Forbidden, "{\"code\":\"company_access_revoked\"}", "https://api.test/api/v1/x");

        Assert.Equal(ApiProblemKind.CompanyAccessRevoked, raised[^1]);
        Assert.Null(session.BearerToken);
        Assert.False(authorization.IsGranted("crm.contacts.view"));
        Assert.Null(await cache.LoadAsync(TenantA, UserA, NoCompany));
    }

    /// <summary>
    /// Verifies that the handler sends the bearer token.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task ApiSessionHandler_SendsTheBearerToken()
    {
        var (session, _, _, _) = NewSession();
        session.Start("abc");
        var inner = new StubHandler(HttpStatusCode.OK, "{}");
#pragma warning disable IDISP014 // One short-lived client per scripted response is the point of the stub.
        using var handler = new ApiSessionHandler(session) { InnerHandler = inner };
        using var client = new HttpClient(handler);
#pragma warning restore IDISP014

        using var response = await client.GetAsync(new Uri("https://api.test/api/v1/x"));

        Assert.Equal("Bearer abc", inner.LastRequest?.Headers.Authorization?.ToString());
        Assert.True(response.IsSuccessStatusCode);
    }

    private static PermissionSnapshot Snapshot(Guid tenant, Guid user, Guid? company, string[] permissions) =>
        new(tenant, user, company, permissions, [], DateTimeOffset.UtcNow);

    private static async Task<HttpResponseMessage> Send(IApiSession session, HttpStatusCode status, string body, string url)
    {
#pragma warning disable IDISP014 // One short-lived client per scripted response is the point of the stub.
        using var handler = new ApiSessionHandler(session) { InnerHandler = new StubHandler(status, body) };
        using var client = new HttpClient(handler);
#pragma warning restore IDISP014
        return await client.GetAsync(new Uri(url));
    }

    private SqlitePermissionCache NewCache() =>
        new($"Data Source={_databasePath}", new AesGcmFieldEncryptor(RandomNumberGenerator.GetBytes(32)));

    private (ApiSession Session, AuthorizationService Authorization, SqlitePermissionCache Cache, List<ApiProblemKind> Raised) NewSession()
    {
        var authorization = new AuthorizationService();
        var cache = NewCache();
        var user = new CurrentUserService();
        user.SignIn(new AuthenticatedUser(UserA, "giulia", "Giulia", IsSystemAdministrator: false, new Dictionary<Guid, UserRole>()));
        var session = new ApiSession(authorization, cache, user);
        var raised = new List<ApiProblemKind>();
        session.ProblemRaised += (_, args) => raised.Add(args.Kind);

        return (session, authorization, cache, raised);
    }

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }
}
