// <copyright file="AccountingDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Accounting.Domain.Aggregates;
using FatturaFacile.Infrastructure.Common.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Accounting.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Accounting bounded context.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AccountingDbContext"/> class.
/// </remarks>
/// <param name="options">The database context options.</param>
/// <param name="tenantContext">The optional ambient tenant context accessor.</param>
public class AccountingDbContext(DbContextOptions<AccountingDbContext> options, ITenantContext? tenantContext = null) : DbContext(options)
{
    private readonly ITenantContext? _tenantContext = tenantContext;

    /// <summary>
    /// Gets the database set for general ledger journal entries.
    /// </summary>
    public DbSet<GeneralLedgerEntry> GeneralLedgerEntries => Set<GeneralLedgerEntry>();

    /// <summary>
    /// Gets the database set for individual general ledger journal lines.
    /// </summary>
    public DbSet<GeneralLedgerLine> GeneralLedgerLines => Set<GeneralLedgerLine>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<GeneralLedgerEntry>(b =>
        {
            _ = b.ToTable("general_ledger_entries");
            _ = b.HasKey(g => g.Id);
            _ = b.Property(g => g.EntryNumber).HasMaxLength(50).IsRequired();
            _ = b.Property(g => g.Description).HasMaxLength(500).IsRequired();
            _ = b.Property(g => g.TotalDebit).HasPrecision(18, 2);
            _ = b.Property(g => g.TotalCredit).HasPrecision(18, 2);

            _ = b.HasMany(g => g.Lines)
                .WithOne()
                .HasForeignKey(l => l.GeneralLedgerEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            if (_tenantContext != null)
            {
                _ = b.HasQueryFilter(g => !g.IsDeleted && (_tenantContext.IsTenantFilterIgnored || g.TenantId == _tenantContext.TenantId));
            }
        });

        _ = modelBuilder.Entity<GeneralLedgerLine>(b =>
        {
            _ = b.ToTable("general_ledger_lines");
            _ = b.HasKey(l => l.Id);
            _ = b.Property(l => l.AccountCode).HasMaxLength(20).IsRequired();
            _ = b.Property(l => l.Description).HasMaxLength(255).IsRequired();
            _ = b.Property(l => l.Amount).HasPrecision(18, 2);

            if (_tenantContext != null)
            {
                _ = b.HasQueryFilter(l => _tenantContext.IsTenantFilterIgnored || l.TenantId == _tenantContext.TenantId);
            }
        });
    }
}
