// <copyright file="LedgerModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Accounting;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// EF Core mapping of the Prima Nota extensions: ledger book, posting and reversal link of the entries, cost center of the lines
/// and the cached accounting templates.
/// </summary>
public static class LedgerModelConfiguration
{
    /// <summary>
    /// Maps the ledger columns and the template cache. The tenant filter of the templates is added by the database context itself.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureLedger(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<GeneralLedgerEntry>(b =>
        {
            _ = b.Property(g => g.LedgerType).HasColumnName("ledger_type").HasConversion<string>().IsRequired().HasMaxLength(20);
            _ = b.Property(g => g.PostedAt).HasColumnName("posted_at");
            _ = b.Property(g => g.ReversalOfEntryId).HasColumnName("reversal_of_entry_id").HasMaxLength(36);
            _ = b.Property(g => g.ReversalReason).HasColumnName("reversal_reason").HasMaxLength(255);

            // An entry is reversed at most once: the storno links to the original, which is never touched.
            _ = b.HasIndex(g => g.ReversalOfEntryId).IsUnique().HasFilter("reversal_of_entry_id IS NOT NULL").HasDatabaseName("ux_general_ledger_entries_reversal_of");
            _ = b.HasIndex(g => new { g.TenantId, g.LedgerType, g.PostingDate }).HasDatabaseName("ix_general_ledger_entries_ledger_date");
        });

        _ = modelBuilder.Entity<GeneralLedgerLine>(b => b.Property(l => l.CostCenterId).HasColumnName("cost_center_id").HasMaxLength(36));

        _ = modelBuilder.Entity<AccountingTemplate>(b =>
        {
            _ = b.ToTable("accounting_templates");
            _ = b.HasKey(t => t.Id);
            _ = b.Property(t => t.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(t => t.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(t => t.Code).HasColumnName("code").IsRequired().HasMaxLength(50);
            _ = b.Property(t => t.Category).HasColumnName("category").IsRequired().HasMaxLength(30);
            _ = b.Property(t => t.LedgerType).HasColumnName("ledger_type").HasConversion<string>().IsRequired().HasMaxLength(20);
            _ = b.Property(t => t.NamesJson).HasColumnName("names_json").IsRequired();
            _ = b.Property(t => t.ParametersJson).HasColumnName("parameters_json").IsRequired();
            _ = b.Property(t => t.LinesJson).HasColumnName("lines_json").IsRequired();
            _ = b.Property(t => t.IsSystem).HasColumnName("is_system");
            _ = b.Property(t => t.SyncedAt).HasColumnName("synced_at");
            _ = b.Property(t => t.CreatedAt).HasColumnName("created_at");
            _ = b.Property(t => t.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(t => new { t.TenantId, t.Code }).IsUnique();
        });
    }
}
