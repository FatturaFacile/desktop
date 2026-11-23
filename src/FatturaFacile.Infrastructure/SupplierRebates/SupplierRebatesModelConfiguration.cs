// <copyright file="SupplierRebatesModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.SupplierRebates;

/// <summary>
/// EF Core mapping of the supplier rebate payload cache.
/// </summary>
public static class SupplierRebatesModelConfiguration
{
    /// <summary>
    /// Maps the snapshot table: one row per tenant, kind and record. The tenant filter is added by the database context itself, so it
    /// reads the tenant service of each context instance.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureSupplierRebates(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<LocalSupplierRebateSnapshot>(b =>
        {
            _ = b.ToTable("supplier_rebate_snapshots");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.Kind).HasColumnName("kind").IsRequired().HasMaxLength(32);
            _ = b.Property(x => x.RecordKey).HasColumnName("record_key").IsRequired().HasMaxLength(64);
            _ = b.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.Kind, x.RecordKey }).IsUnique();
        });
    }
}
