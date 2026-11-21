// <copyright file="ManufacturingHarness.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>
#pragma warning disable SA1600 // Elements should be documented: internal test double.

using System.Globalization;
using System.Text.Json;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Manufacturing;
using FatturaFacile.Infrastructure.Manufacturing;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FatturaFacile.UnitTests.Manufacturing;

/// <summary>
/// A real manufacturing store (SQLite in memory, created through the shipped migration) wired to a mocked web-api gateway.
/// </summary>
internal sealed class ManufacturingHarness : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    public ManufacturingHarness()
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<ManufacturingDbContext>().UseSqlite(connection).Options;
        Factory = new TestDbContextFactory(options, Tenant);
        using (var db = Factory.CreateDbContext())
        {
            db.Database.Migrate();
        }

        _ = Gateway.SetupGet(g => g.IsAuthenticated).Returns(value: true);
        Store = new WorkbenchStore(Factory, Tenant, Clock);
        Queue = new OutboxSyncQueue(Store, Store, Gateway.Object, Tenant, Clock);
    }

    public CurrentTenantService Tenant { get; } = new();

    public ManualTimeProvider Clock { get; } = new();

    public Mock<IManufacturingApiGateway> Gateway { get; } = new();

    public TestDbContextFactory Factory { get; }

    public WorkbenchStore Store { get; }

    public OutboxSyncQueue Queue { get; }

    public static ManufacturingSendOutcome Outcome(SyncSendResult result, int status, string? body = null, string? error = null) =>
        new(result, status, error, body);

    public static JsonElement Json(string text)
    {
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    public void Dispose() => connection.Dispose();

    public async Task<ProductionOrder> SeedOrderAsync(ProductionOrderStatus status, bool synced = true, int operations = 2)
    {
        var id = Guid.NewGuid();
        ProductionOrder order = new()
        {
            Id = id,
            TenantId = Tenant.TenantId,
            ServerId = synced ? id : null,
            OrderNumber = "MO-" + id.ToString("N")[..6],
            ItemId = Guid.NewGuid(),
            ItemCode = "ITEM",
            Status = status,
            TargetQuantity = 10m,
            PlannedStartDate = Clock.GetUtcNow().Date,
            PlannedEndDate = Clock.GetUtcNow().Date.AddDays(3),
        };

        for (var i = 1; i <= operations; i++)
        {
            order.Operations.Add(new ProductionOperation
            {
                TenantId = Tenant.TenantId,
                ProductionOrderId = id,
                Sequence = i * 10,
                OperationCode = "OP" + (i * 10).ToString(CultureInfo.InvariantCulture),
                Name = "Operation " + (i * 10).ToString(CultureInfo.InvariantCulture),
                WorkCenterId = Guid.NewGuid(),
                PlannedRunMinutes = 60m,
            });
        }

        await using var db = Factory.CreateDbContext();
        _ = db.ProductionOrders.Add(order);
        _ = await db.SaveChangesAsync();
        return order;
    }

    public async Task<ProductionOrder> OrderAsync(Guid id)
    {
        await using var db = Factory.CreateDbContext();
        return await db.ProductionOrders.AsNoTracking().Include(o => o.Operations).SingleAsync(o => o.Id == id);
    }

    public async Task<List<ManufacturingOutboxEntry>> OutboxAsync()
    {
        await using var db = Factory.CreateDbContext();
        return await db.Outbox.AsNoTracking().OrderBy(e => e.Sequence).ToListAsync();
    }

    public async Task<List<ShopFloorLog>> LogsAsync()
    {
        await using var db = Factory.CreateDbContext();
        return await db.ShopFloorLogs.AsNoTracking().ToListAsync();
    }

    public List<ManufacturingRequest> SentRequests()
    {
        List<ManufacturingRequest> sent = [];
        foreach (var invocation in Gateway.Invocations)
        {
            if (string.Equals(invocation.Method.Name, nameof(IManufacturingApiGateway.SendAsync), StringComparison.Ordinal) && invocation.Arguments[0] is ManufacturingRequest request)
            {
                sent.Add(request);
            }
        }

        return sent;
    }

    public sealed class TestDbContextFactory(DbContextOptions<ManufacturingDbContext> options, CurrentTenantService tenant) : IDbContextFactory<ManufacturingDbContext>
    {
        public ManufacturingDbContext CreateDbContext() => new(options, tenant);
    }
}
