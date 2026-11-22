// <copyright file="ProjectEvmModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.ProjectEvm;

/// <summary>
/// EF Core mapping of the project profitability snapshot cache.
/// </summary>
public static class ProjectEvmModelConfiguration
{
    /// <summary>
    /// Maps the snapshot table: one row per tenant and project. The tenant filter is added by the database context itself, so it
    /// reads the tenant service of each context instance.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureProjectEvm(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<LocalProjectEvmSnapshot>(b =>
        {
            _ = b.ToTable("project_evm_snapshots");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.ProjectId).HasColumnName("project_id").IsRequired().HasMaxLength(36);
            _ = b.Property(x => x.BreakdownJson).HasColumnName("breakdown_json").IsRequired();
            _ = b.Property(x => x.WaterfallJson).HasColumnName("waterfall_json").IsRequired();
            _ = b.Property(x => x.ScurveJson).HasColumnName("scurve_json").IsRequired();
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.ProjectId }).IsUnique();
        });
    }
}
