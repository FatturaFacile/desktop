// <copyright file="ProjectBillingSnapshotStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ProjectBilling;
using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.ProjectBilling;
using FatturaFacile.Infrastructure.ProjectEvm;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local project billing cache: round trip, one row per project, replacement, tenant isolation and
/// independence from the profitability row of the same project.
/// </summary>
public sealed class ProjectBillingSnapshotStoreTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly ProjectBillingSnapshotStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBillingSnapshotStoreTests"/> class.
    /// </summary>
    public ProjectBillingSnapshotStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        store = new ProjectBillingSnapshotStore(db, tenant);
    }

    /// <summary>
    /// A saved snapshot comes back as it was stored, with its timestamp.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveAndFind_RoundTripsThePayload()
    {
        var at = new DateTime(2027, 3, 1, 12, 0, 0, DateTimeKind.Utc);

        await store.SaveAsync(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), at, CancellationToken.None);
        var stored = await store.FindAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(at, stored.SyncedAtUtc);
        var snapshot = ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, stored.Payload, stored.SyncedAtUtc);
        Assert.Equal(2, snapshot.Certificates.Count);
        Assert.Equal(1000m, snapshot.Retention?.Balance);
        Assert.True(snapshot.Retainer?.IsLow);
        Assert.Equal(100_000m, snapshot.Contract?.ContractValue);
    }

    /// <summary>
    /// The absent optional views survive the round trip as absent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveAndFind_ProjectWithoutContract_KeepsTheOptionalViewsAbsent()
    {
        await store.SaveAsync(ProjectBillingTestData.ProjectId, ProjectBillingTestData.PayloadWithoutContract(), DateTime.UtcNow, CancellationToken.None);
        var stored = await store.FindAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(ProjectBillingTestData.Absent, stored.Payload.ContractJson);
        Assert.Equal(ProjectBillingTestData.Absent, stored.Payload.RetentionJson);
        Assert.Equal(ProjectBillingTestData.Absent, stored.Payload.RetainerJson);
        Assert.Equal("[]", stored.Payload.CertificatesJson);
    }

    /// <summary>
    /// Nothing is found for a project that was never pulled.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Find_BeforeTheFirstSync_ReturnsNull() =>
        Assert.Null(await store.FindAsync(ProjectBillingTestData.ProjectId, CancellationToken.None));

    /// <summary>
    /// A second save replaces the first: one row per project.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Twice_KeepsOneRowWithTheLatestPayload()
    {
        var first = new DateTime(2027, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        var second = first.AddHours(4);

        await store.SaveAsync(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), first, CancellationToken.None);
        await store.SaveAsync(ProjectBillingTestData.ProjectId, ProjectBillingTestData.PayloadWithoutContract(), second, CancellationToken.None);

        var stored = await store.FindAsync(ProjectBillingTestData.ProjectId, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(second, stored.SyncedAtUtc);
        Assert.Equal(ProjectBillingTestData.Absent, stored.Payload.ContractJson);
        Assert.Equal(1, await db.Set<LocalProjectEvmSnapshot>().CountAsync());
    }

    /// <summary>
    /// A tenant never sees the billing snapshot of another.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Find_OtherTenant_ReturnsNull()
    {
        await store.SaveAsync(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), DateTime.UtcNow, CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");

        Assert.Null(await store.FindAsync(ProjectBillingTestData.ProjectId, CancellationToken.None));
    }

    /// <summary>
    /// The billing row and the profitability row of the same project live side by side.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_SameProjectAsTheProfitabilityView_DoesNotCollide()
    {
        var evm = new ProjectEvmSnapshotStore(db, tenant);
        await evm.SaveAsync(ProjectBillingTestData.ProjectId, new ProjectEvmPayload("{}", "{}", "{}"), DateTime.UtcNow, CancellationToken.None);

        await store.SaveAsync(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), DateTime.UtcNow, CancellationToken.None);

        Assert.NotNull(await evm.FindAsync(ProjectBillingTestData.ProjectId, CancellationToken.None));
        Assert.NotNull(await store.FindAsync(ProjectBillingTestData.ProjectId, CancellationToken.None));
        Assert.Equal(2, await db.Set<LocalProjectEvmSnapshot>().CountAsync());
    }

    /// <summary>
    /// The key of a project fits the column, whatever the identifier.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    [Theory]
    [InlineData("11111111-2222-3333-4444-555555555555")]
    [InlineData("a-very-long-project-identifier-that-is-not-a-guid-at-all-0123456789")]
    public void KeyOf_FitsTheColumn(string projectId)
    {
        var key = ProjectBillingSnapshotStore.KeyOf(projectId);

        Assert.True(key.Length <= 36);
        Assert.NotEqual(projectId, key, StringComparer.Ordinal);
        Assert.Equal(key, ProjectBillingSnapshotStore.KeyOf(projectId));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }
}
