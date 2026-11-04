using Microsoft.EntityFrameworkCore;
using FatturaFacile.Accounting.Domain.Aggregates;
using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Accounting.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Accounting bounded context.
/// </summary>
public class AccountingDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public DbSet<GeneralLedgerEntry> GeneralLedgerEntries => Set<GeneralLedgerEntry>();
    public DbSet<GeneralLedgerLine> GeneralLedgerLines => Set<GeneralLedgerLine>();

    public AccountingDbContext(DbContextOptions<AccountingDbContext> options, ITenantContext? tenantContext = null) 
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GeneralLedgerEntry>(b =>
        {
            b.ToTable("general_ledger_entries");
            b.HasKey(g => g.Id);
            b.Property(g => g.EntryNumber).HasMaxLength(50).IsRequired();
            b.Property(g => g.Description).HasMaxLength(500).IsRequired();
            b.Property(g => g.TotalDebit).HasPrecision(18, 2);
            b.Property(g => g.TotalCredit).HasPrecision(18, 2);

            b.HasMany(g => g.Lines)
                .WithOne()
                .HasForeignKey(l => l.GeneralLedgerEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            if (_tenantContext != null)
            {
                b.HasQueryFilter(g => !g.IsDeleted && (_tenantContext.IsTenantFilterIgnored || g.TenantId == _tenantContext.TenantId));
            }
        });

        modelBuilder.Entity<GeneralLedgerLine>(b =>
        {
            b.ToTable("general_ledger_lines");
            b.HasKey(l => l.Id);
            b.Property(l => l.AccountCode).HasMaxLength(50).IsRequired();
            b.Property(l => l.Description).HasMaxLength(500);
            b.Property(l => l.Amount).HasPrecision(18, 2);
            b.Property(l => l.Type).HasConversion<string>().HasMaxLength(20);

            if (_tenantContext != null)
            {
                b.HasQueryFilter(l => _tenantContext.IsTenantFilterIgnored || l.TenantId == _tenantContext.TenantId);
            }
        });
    }
}
