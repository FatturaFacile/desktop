// <copyright file="DssSnapshotStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Dss;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local Executive DSS snapshot cache: round trip, one row per filter, replacement and tenant isolation.
/// </summary>
public sealed class DssSnapshotStoreTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly DssSnapshotStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="DssSnapshotStoreTests"/> class.
    /// </summary>
    public DssSnapshotStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        store = new DssSnapshotStore(db, tenant);
    }

    /// <summary>
    /// A saved snapshot comes back as it was stored, with its timestamp.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveAndFind_RoundTripsThePayload()
    {
        var at = new DateTime(2026, 6, 30, 12, 0, 0, DateTimeKind.Utc);

        await store.SaveAsync(DssFilter.Default, DssTestData.Payload(), at, CancellationToken.None);
        var stored = await store.FindAsync(DssFilter.Default, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(at, stored.SyncedAtUtc);
        Assert.Equal(DssTestData.Overview, stored.Payload.OverviewJson);
        Assert.Equal(DssTestData.Profitability, stored.Payload.ProfitabilityJson);
        Assert.Equal(DssTestData.VarianceBridge, stored.Payload.VarianceBridgeJson);
        Assert.Equal(DssTestData.SimulationInputs, stored.Payload.SimulationInputsJson);
        Assert.Equal(DssTestData.LegalEntities, stored.Payload.LegalEntitiesJson);
    }

    /// <summary>
    /// Nothing is found for a filter that was never pulled.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Find_UnknownFilter_ReturnsNull() =>
        Assert.Null(await store.FindAsync(DssFilter.Default, CancellationToken.None));

    /// <summary>
    /// The optional pieces are kept as null.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_WithoutOptionalPieces_StoresNulls()
    {
        await store.SaveAsync(DssFilter.Default, DssTestData.Payload(inputs: null, entities: null), DateTime.UtcNow, CancellationToken.None);

        var stored = await store.FindAsync(DssFilter.Default, CancellationToken.None);

        Assert.Null(stored!.Payload.SimulationInputsJson);
        Assert.Null(stored.Payload.LegalEntitiesJson);
    }

    /// <summary>
    /// Every part of the filter is a part of the key: entity, currency, period and baseline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_DifferentFilters_AreKeptApart()
    {
        DssFilter[] filters =
        [
            DssFilter.Default,
            DssFilter.Default with { LegalEntityId = "ent-1" },
            DssFilter.Default with { Currency = "EUR" },
            DssFilter.Default with { PeriodPreset = "MTD" },
            DssFilter.Default with { ComparisonBaseline = "BUDGET" },
        ];

        foreach (var filter in filters)
        {
            var payload = DssTestData.Payload() with { OverviewJson = "{\"for\":\"" + filter.LegalEntityId + filter.Currency + filter.PeriodPreset + filter.ComparisonBaseline + "\"}" };
            await store.SaveAsync(filter, payload, DateTime.UtcNow, CancellationToken.None);
        }

        Assert.Equal(filters.Length, await db.DssSnapshots.CountAsync());
        foreach (var filter in filters)
        {
            var stored = await store.FindAsync(filter, CancellationToken.None);
            Assert.Contains(filter.LegalEntityId + filter.Currency + filter.PeriodPreset + filter.ComparisonBaseline, stored!.Payload.OverviewJson, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// A new pull replaces the snapshot of the filter: one row, the new payload and timestamp.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Twice_ReplacesTheSnapshot()
    {
        var first = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var second = first.AddDays(1);

        await store.SaveAsync(DssFilter.Default, DssTestData.Payload(), first, CancellationToken.None);
        await store.SaveAsync(DssFilter.Default, DssTestData.Payload(entities: null) with { OverviewJson = "{\"v\":2}" }, second, CancellationToken.None);

        var row = await db.DssSnapshots.SingleAsync();
        Assert.Equal(second, row.SyncedAtUtc);
        _ = Assert.NotNull(row.UpdatedAt);
        Assert.Equal("{\"v\":2}", row.OverviewJson);
        Assert.Null(row.LegalEntitiesJson);
    }

    /// <summary>
    /// Another tenant never sees the snapshot of this tenant, and its own snapshot of the same filter is separate.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Find_OtherTenant_DoesNotSeeTheSnapshot()
    {
        await store.SaveAsync(DssFilter.Default, DssTestData.Payload(), DateTime.UtcNow, CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "IT", "EUR");
        Assert.Null(await store.FindAsync(DssFilter.Default, CancellationToken.None));

        await store.SaveAsync(DssFilter.Default, DssTestData.Payload() with { OverviewJson = "{\"tenant\":\"b\"}" }, DateTime.UtcNow, CancellationToken.None);
        Assert.Contains("\"b\"", (await store.FindAsync(DssFilter.Default, CancellationToken.None))!.Payload.OverviewJson, StringComparison.Ordinal);

        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        var mine = await store.FindAsync(DssFilter.Default, CancellationToken.None);
        Assert.Equal(DssTestData.Overview, mine!.Payload.OverviewJson);
        Assert.Equal(1, await db.DssSnapshots.CountAsync());
        Assert.Equal(2, await db.DssSnapshots.IgnoreQueryFilters().CountAsync());
    }

    /// <summary>
    /// The row is stamped with the tenant that saved it.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_StampsTheActiveTenant()
    {
        await store.SaveAsync(DssFilter.Default, DssTestData.Payload(), DateTime.UtcNow, CancellationToken.None);

        var row = await db.DssSnapshots.IgnoreQueryFilters().SingleAsync();

        Assert.Equal(TenantA, row.TenantId);
        _ = Assert.IsType<LocalDssSnapshot>(row);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }
}
