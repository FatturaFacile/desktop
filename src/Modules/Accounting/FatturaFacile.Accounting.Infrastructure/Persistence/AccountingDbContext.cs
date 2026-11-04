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
public class AccountingDbContext(
    DbContextOptions<AccountingDbContext> options,
    ITenantContext? tenantContext = null)
    : DbContext(options)
{
    private const int EntryNumberMaxLength = 50;
    private const int EntryDescriptionMaxLength = 500;
    private const int AccountCodeMaxLength = 20;
    private const int LineDescriptionMaxLength = 255;

    private const int DecimalPrecision = 18;
    private const int DecimalScale = 2;

    private readonly ITenantContext? _tenantContext = tenantContext;

    /// <summary>
    /// Gets the database set for general ledger journal entries.
    /// </summary>
    public DbSet<GeneralLedgerEntry> GeneralLedgerEntries =>
        Set<GeneralLedgerEntry>();

    /// <summary>
    /// Gets the database set for individual general ledger journal lines.
    /// </summary>
    public DbSet<GeneralLedgerLine> GeneralLedgerLines =>
        Set<GeneralLedgerLine>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        ConfigureGeneralLedgerEntry(modelBuilder);
        ConfigureGeneralLedgerLine(modelBuilder);
    }

    private void ConfigureGeneralLedgerEntry(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<GeneralLedgerEntry>(builder =>
        {
            _ = builder.ToTable("general_ledger_entries");

            _ = builder.HasKey(entity => entity.Id);

            _ = builder.Property(entity => entity.EntryNumber)
                .HasMaxLength(EntryNumberMaxLength)
                .IsRequired();

            _ = builder.Property(entity => entity.Description)
                .HasMaxLength(EntryDescriptionMaxLength)
                .IsRequired();

            _ = builder.Property(entity => entity.TotalDebit)
                .HasPrecision(DecimalPrecision, DecimalScale);

            _ = builder.Property(entity => entity.TotalCredit)
                .HasPrecision(DecimalPrecision, DecimalScale);

            _ = builder.HasMany(entity => entity.Lines)
                .WithOne()
                .HasForeignKey(line => line.GeneralLedgerEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            if (_tenantContext is not null)
            {
                _ = builder.HasQueryFilter(entity =>
                    !entity.IsDeleted &&
                    (_tenantContext.IsTenantFilterIgnored ||
                     entity.TenantId == _tenantContext.TenantId));
            }
        });
    }

    private void ConfigureGeneralLedgerLine(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<GeneralLedgerLine>(builder =>
        {
            _ = builder.ToTable("general_ledger_lines");

            _ = builder.HasKey(entity => entity.Id);

            _ = builder.Property(entity => entity.AccountCode)
                .HasMaxLength(AccountCodeMaxLength)
                .IsRequired();

            _ = builder.Property(entity => entity.Description)
                .HasMaxLength(LineDescriptionMaxLength)
                .IsRequired();

            _ = builder.Property(entity => entity.Amount)
                .HasPrecision(DecimalPrecision, DecimalScale);

            if (_tenantContext is not null)
            {
                _ = builder.HasQueryFilter(entity =>
                    _tenantContext.IsTenantFilterIgnored ||
                    entity.TenantId == _tenantContext.TenantId);
            }
        });
    }
}
