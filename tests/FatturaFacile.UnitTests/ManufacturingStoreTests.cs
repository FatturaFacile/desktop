// <copyright file="ManufacturingStoreTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Manufacturing;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// SQLite tests of the local manufacturing cache: round trip with children, atomic replacement and tenant isolation.
/// </summary>
public sealed class ManufacturingStoreTests : IDisposable
{
    private static readonly Guid TenantA = new(0xAAAAAAAA, 0xAAAA, 0xAAAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA);
    private static readonly Guid TenantB = new(0xBBBBBBBB, 0xBBBB, 0xBBBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB, 0xBB);
    private static readonly DateTime At = new(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly CurrentTenantService tenant = new();
    private readonly FatturaFacileDbContext db;
    private readonly ManufacturingStore store;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManufacturingStoreTests"/> class.
    /// </summary>
    public ManufacturingStoreTests()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(tenant))
            .Options;
        db = new FatturaFacileDbContext(options, tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = db.Database.EnsureCreated();
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        store = new ManufacturingStore(db, tenant);
    }

    /// <summary>
    /// Everything that was stored comes back, with the operation steps, BOM links, shifts and closures.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ReplaceAndRead_RoundTripsTheMasterData()
    {
        await store.ReplaceAsync(Pulled(), CancellationToken.None);

        var overview = await store.GetOverviewAsync(CancellationToken.None);

        Assert.Equal(["WC-01", "WC-02"], overview.WorkCenters.Select(w => w.WorkCenterCode));
        Assert.All(overview.WorkCenters, w => Assert.Equal(TenantA, w.TenantId));
        var machine = Assert.Single(overview.Machines);
        Assert.Equal(30m, machine.IdealCycleTimeSecondsPerUnit);
        var calendar = Assert.Single(overview.Calendars);
        Assert.Equal(2, calendar.Shifts.Count);
        _ = Assert.Single(calendar.Closures);
        var routing = Assert.Single(overview.Routings);
        Assert.Equal([10, 20], routing.Operations.Select(o => o.Sequence));
        var mill = routing.Operations.First();
        Assert.Equal(ManufacturingTestData.WorkInstruction, mill.WorkInstructionDocId);
        Assert.Equal(5m, mill.OverlapTransferBatchQty);
        var link = Assert.Single(mill.BomComponents);
        Assert.Equal(ManufacturingTestData.BomComponent, link.BomComponentId);
        var oee = Assert.Single(overview.OeeSnapshots);
        Assert.Equal(66.67d, oee.OeePercent);
        Assert.Equal(At, overview.LastSyncedUtc);
    }

    /// <summary>
    /// Before the first sync the cache is empty and has no sync stamp.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Read_BeforeAnySync_IsEmpty()
    {
        var overview = await store.GetOverviewAsync(CancellationToken.None);

        Assert.Empty(overview.WorkCenters);
        Assert.Empty(overview.Routings);
        Assert.Empty(overview.OeeSnapshots);
        Assert.Null(overview.LastSyncedUtc);
    }

    /// <summary>
    /// A second pull replaces the first one entirely: changed records are updated and records the server dropped disappear.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Replace_Twice_KeepsOnlyTheLastPull()
    {
        await store.ReplaceAsync(Pulled(), CancellationToken.None);
        var later = At.AddHours(1);
        var second = new ManufacturingMasterData(
            [.. ManufacturingParser.ParseWorkCenters(ManufacturingTestData.WorkCenters, later).Take(1)],
            [],
            [],
            ManufacturingParser.ParseRoutings(ManufacturingTestData.Routings, later),
            []);

        await store.ReplaceAsync(second, CancellationToken.None);
        var overview = await store.GetOverviewAsync(CancellationToken.None);

        _ = Assert.Single(overview.WorkCenters);
        Assert.Empty(overview.Machines);
        Assert.Empty(overview.Calendars);
        Assert.Empty(overview.OeeSnapshots);
        Assert.Equal(2, Assert.Single(overview.Routings).Operations.Count);
        Assert.Equal(later, overview.LastSyncedUtc);
        Assert.Equal(1, await db.Set<MfgRoutingOperationBomLink>().CountAsync());
        Assert.Equal(0, await db.Set<MfgCalendarShift>().CountAsync());
    }

    /// <summary>
    /// A tenant never sees, and a replace never touches, the cache of another tenant.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Tenants_AreIsolated()
    {
        await store.ReplaceAsync(Pulled(), CancellationToken.None);

        tenant.SetCurrentTenant(TenantB, "Beta", "CH", "CHF");
        Assert.Empty((await store.GetOverviewAsync(CancellationToken.None)).WorkCenters);
        var betaCenter = ManufacturingParser.ParseWorkCenters(ManufacturingTestData.WorkCenters, At)[0];
        betaCenter.Id = Guid.NewGuid();
        await store.ReplaceAsync(new ManufacturingMasterData([betaCenter], [], [], [], []), CancellationToken.None);
        db.ChangeTracker.Clear();

        var beta = await store.GetOverviewAsync(CancellationToken.None);
        tenant.SetCurrentTenant(TenantA, "Alpha", "CH", "CHF");
        var alpha = await store.GetOverviewAsync(CancellationToken.None);

        _ = Assert.Single(beta.WorkCenters);
        Assert.Equal(2, alpha.WorkCenters.Count);
        _ = Assert.Single(alpha.Routings);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private static ManufacturingMasterData Pulled() => new(
        ManufacturingParser.ParseWorkCenters(ManufacturingTestData.WorkCenters, At),
        ManufacturingParser.ParseMachines(ManufacturingTestData.Machines, At),
        ManufacturingParser.ParseCalendars(ManufacturingTestData.Calendars, At),
        ManufacturingParser.ParseRoutings(ManufacturingTestData.Routings, At),
        [ManufacturingParser.ParseOeeSnapshot(ManufacturingTestData.OeeSummary, At)]);
}
