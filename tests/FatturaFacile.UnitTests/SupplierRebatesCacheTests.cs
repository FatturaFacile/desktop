// <copyright file="SupplierRebatesCacheTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using FatturaFacile.Infrastructure.SupplierRebates;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local supplier rebate cache: round trip, one row per kind and record, replacement and tenant isolation.
/// </summary>
public sealed class SupplierRebatesCacheTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly SupplierRebatesCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierRebatesCacheTests"/> class.
    /// </summary>
    public SupplierRebatesCacheTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        cache = new SupplierRebatesCache(db, tenant);
    }

    /// <summary>
    /// Saved payloads come back as they were stored, with their timestamp.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveAndFind_RoundTripsThePayloads()
    {
        var at = new DateTime(2027, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        await cache.SaveAsync(
            [
                new RebateCacheEntry(RebateCacheKinds.Agreement, SupplierRebatesTestData.AgreementId, SupplierRebatesTestData.AgreementJson),
                new RebateCacheEntry(RebateCacheKinds.Progress, SupplierRebatesTestData.AgreementId, SupplierRebatesTestData.ProgressJson),
            ],
            at,
            CancellationToken.None);

        var agreement = await cache.FindAsync(RebateCacheKinds.Agreement, SupplierRebatesTestData.AgreementId, CancellationToken.None);
        var progress = await cache.FindAsync(RebateCacheKinds.Progress, SupplierRebatesTestData.AgreementId, CancellationToken.None);

        Assert.NotNull(agreement);
        Assert.Equal(SupplierRebatesTestData.AgreementJson, agreement.Json);
        Assert.Equal(at, agreement.SyncedAtUtc);
        Assert.Equal(SupplierRebatesTestData.ProgressJson, progress?.Json);
    }

    /// <summary>
    /// Nothing is found for a payload that was never pulled.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Find_BeforeTheFirstSync_ReturnsNull() =>
        Assert.Null(await cache.FindAsync(RebateCacheKinds.Agreements, RebateCacheKinds.AllKey, CancellationToken.None));

    /// <summary>
    /// A second save replaces the row of the kind and record instead of adding one; the same key under another kind is another row.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Twice_KeepsOneRowPerKindAndRecord()
    {
        var first = new DateTime(2027, 4, 1, 12, 0, 0, DateTimeKind.Utc);

        await cache.SaveAsync([new RebateCacheEntry(RebateCacheKinds.Runs, SupplierRebatesTestData.AgreementId, "[]")], first, CancellationToken.None);
        await cache.SaveAsync(
            [
                new RebateCacheEntry(RebateCacheKinds.Runs, SupplierRebatesTestData.AgreementId, SupplierRebatesTestData.RunsJson),
                new RebateCacheEntry(RebateCacheKinds.Proposals, SupplierRebatesTestData.AgreementId, "[]"),
            ],
            first.AddDays(1),
            CancellationToken.None);

        Assert.Equal(2, await db.Set<LocalSupplierRebateSnapshot>().CountAsync());
        var runs = await cache.FindAsync(RebateCacheKinds.Runs, SupplierRebatesTestData.AgreementId, CancellationToken.None);
        Assert.Equal(SupplierRebatesTestData.RunsJson, runs?.Json);
        Assert.Equal(first.AddDays(1), runs?.SyncedAtUtc);
    }

    /// <summary>
    /// A tenant never sees the payloads of another one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Tenants_AreIsolated()
    {
        await cache.SaveAsync([new RebateCacheEntry(RebateCacheKinds.BlanketBalance, SupplierRebatesTestData.ContractId, SupplierRebatesTestData.BalanceJson)], DateTime.UtcNow, CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");

        Assert.Null(await cache.FindAsync(RebateCacheKinds.BlanketBalance, SupplierRebatesTestData.ContractId, CancellationToken.None));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }
}
