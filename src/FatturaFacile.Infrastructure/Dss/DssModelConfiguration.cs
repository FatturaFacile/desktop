// <copyright file="DssModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Dss;

/// <summary>
/// EF Core mapping of the Executive DSS snapshot cache.
/// </summary>
public static class DssModelConfiguration
{
    /// <summary>
    /// Maps the snapshot table: one row per tenant, legal entity, currency, period and baseline. The tenant filter is added by the
    /// database context itself, so it reads the tenant service of each context instance (a filter closed over here would stay bound to
    /// the instance that first built the cached model).
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureDss(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<LocalDssSnapshot>(b =>
        {
            _ = b.ToTable("dss_snapshots");
            _ = b.HasKey(x => x.Id);
            _ = b.Property(x => x.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(x => x.LegalEntityId).HasColumnName("legal_entity_id").IsRequired().HasMaxLength(36);
            _ = b.Property(x => x.Currency).HasColumnName("currency").IsRequired().HasMaxLength(3);
            _ = b.Property(x => x.PeriodPreset).HasColumnName("period_preset").IsRequired().HasMaxLength(10);
            _ = b.Property(x => x.ComparisonBaseline).HasColumnName("comparison_baseline").IsRequired().HasMaxLength(20);
            _ = b.Property(x => x.OverviewJson).HasColumnName("overview_json").IsRequired();
            _ = b.Property(x => x.ProfitabilityJson).HasColumnName("profitability_json").IsRequired();
            _ = b.Property(x => x.VarianceBridgeJson).HasColumnName("variance_bridge_json").IsRequired();
            _ = b.Property(x => x.SimulationInputsJson).HasColumnName("simulation_inputs_json");
            _ = b.Property(x => x.LegalEntitiesJson).HasColumnName("legal_entities_json");
            _ = b.Property(x => x.SyncedAtUtc).HasColumnName("synced_at");
            _ = b.Property(x => x.CreatedAt).HasColumnName("created_at");
            _ = b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(x => new { x.TenantId, x.LegalEntityId, x.Currency, x.PeriodPreset, x.ComparisonBaseline }).IsUnique();
        });
    }
}
