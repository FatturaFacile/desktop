// <copyright file="ManufacturingDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// The offline-first SQLite store of the engineering and shop-floor workbench (a separate database file, so the ERP schema and its migrations stay untouched).
/// Every table is tenant scoped through a global query filter; the stores stamp the tenant on insert.
/// </summary>
/// <param name="options">The context options.</param>
/// <param name="tenant">The current tenant.</param>
public sealed class ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options, ICurrentTenantService tenant) : DbContext(options)
{
    /// <summary>
    /// Gets the work centres.
    /// </summary>
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();

    /// <summary>
    /// Gets the item standard costs.
    /// </summary>
    public DbSet<ItemCost> ItemCosts => Set<ItemCost>();

    /// <summary>
    /// Gets the BOM headers.
    /// </summary>
    public DbSet<BomHeader> BomHeaders => Set<BomHeader>();

    /// <summary>
    /// Gets the BOM lines.
    /// </summary>
    public DbSet<BomLine> BomLines => Set<BomLine>();

    /// <summary>
    /// Gets the routing operations.
    /// </summary>
    public DbSet<RoutingOperation> RoutingOperations => Set<RoutingOperation>();

    /// <summary>
    /// Gets the MRP planned orders.
    /// </summary>
    public DbSet<MrpPlan> MrpPlans => Set<MrpPlan>();

    /// <summary>
    /// Gets the production orders.
    /// </summary>
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    /// <summary>
    /// Gets the production order operations.
    /// </summary>
    public DbSet<ProductionOperation> ProductionOperations => Set<ProductionOperation>();

    /// <summary>
    /// Gets the shop-floor logs.
    /// </summary>
    public DbSet<ShopFloorLog> ShopFloorLogs => Set<ShopFloorLog>();

    /// <summary>
    /// Gets the cached cockpit snapshots.
    /// </summary>
    public DbSet<TelemetrySnapshot> TelemetrySnapshots => Set<TelemetrySnapshot>();

    /// <summary>
    /// Gets the outbox.
    /// </summary>
    public DbSet<ManufacturingOutboxEntry> Outbox => Set<ManufacturingOutboxEntry>();

    /// <summary>
    /// Gets the delta-sync checkpoints.
    /// </summary>
    public DbSet<ManufacturingCheckpoint> Checkpoints => Set<ManufacturingCheckpoint>();

    private Guid CurrentTenantId => tenant.TenantId;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<WorkCenter>(b =>
        {
            _ = b.ToTable("MfgWorkCenters").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.Code });
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<ItemCost>(b =>
        {
            _ = b.ToTable("MfgItemCosts").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.ItemCode });
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<BomHeader>(b =>
        {
            _ = b.ToTable("MfgBomHeaders").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.ItemId });
            _ = b.HasMany(e => e.Lines).WithOne().HasForeignKey(l => l.BomHeaderId).OnDelete(DeleteBehavior.Cascade);
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<BomLine>(b =>
        {
            _ = b.ToTable("MfgBomLines").HasKey(e => e.Id);
            _ = b.HasIndex(e => e.ComponentItemId);
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<RoutingOperation>(b =>
        {
            _ = b.ToTable("MfgRoutingOperations").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.RoutingId, e.Sequence });
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<MrpPlan>(b =>
        {
            _ = b.ToTable("MfgMrpPlans").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.DueDate });
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<ProductionOrder>(b =>
        {
            _ = b.ToTable("MfgProductionOrders").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.ServerId });
            _ = b.HasIndex(e => new { e.TenantId, e.Status });
            _ = b.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            _ = b.HasMany(e => e.Operations).WithOne().HasForeignKey(o => o.ProductionOrderId).OnDelete(DeleteBehavior.Cascade);
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<ProductionOperation>(b =>
        {
            _ = b.ToTable("MfgProductionOperations").HasKey(e => e.Id);
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<ShopFloorLog>(b =>
        {
            _ = b.ToTable("MfgShopFloorLogs").HasKey(e => e.Id);
            _ = b.HasIndex(e => e.OperationPostingUuid).IsUnique();
            _ = b.HasIndex(e => new { e.TenantId, e.ProductionOrderId });
            _ = b.Property(e => e.SyncStatus).HasConversion<string>().HasMaxLength(20);
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<TelemetrySnapshot>(b =>
        {
            _ = b.ToTable("MfgTelemetrySnapshots").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.CapturedAt });
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<ManufacturingOutboxEntry>(b =>
        {
            _ = b.ToTable("MfgOutbox").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.Status, e.Sequence });
            _ = b.HasIndex(e => e.IdempotencyKey).IsUnique();
            _ = b.Property(e => e.Kind).HasConversion<string>().HasMaxLength(24);
            _ = b.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        _ = modelBuilder.Entity<ManufacturingCheckpoint>(b =>
        {
            _ = b.ToTable("MfgCheckpoints").HasKey(e => e.Id);
            _ = b.HasIndex(e => new { e.TenantId, e.Resource }).IsUnique();
            _ = b.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
    }
}
