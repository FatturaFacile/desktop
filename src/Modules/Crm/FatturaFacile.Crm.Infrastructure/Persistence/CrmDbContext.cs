using Microsoft.EntityFrameworkCore;
using FatturaFacile.Crm.Domain.Aggregates;
using FatturaFacile.Infrastructure.Common.Tenancy;

namespace FatturaFacile.Crm.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the CRM bounded context.
/// </summary>
public class CrmDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public DbSet<Contact> Contacts => Set<Contact>();

    public CrmDbContext(DbContextOptions<CrmDbContext> options, ITenantContext? tenantContext = null) 
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>(b =>
        {
            b.ToTable("contacts");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).ValueGeneratedNever();
            b.Property(c => c.Name).HasMaxLength(255).IsRequired();
            b.Property(c => c.TradeName).HasMaxLength(255);
            b.Property(c => c.VatNumber).HasMaxLength(50);
            b.Property(c => c.TaxCode).HasMaxLength(50);
            b.Property(c => c.Email).HasMaxLength(255);
            b.Property(c => c.Phone).HasMaxLength(50);
            b.Property(c => c.Pec).HasMaxLength(255);
            b.Property(c => c.SdiCode).HasMaxLength(20);
            b.Property(c => c.CountryCode).HasMaxLength(2).IsRequired();
            b.Property(c => c.ContactType).HasConversion<string>().HasMaxLength(30);

            b.HasIndex(c => new { c.TenantId, c.Name });
            b.HasIndex(c => new { c.TenantId, c.VatNumber });

            if (_tenantContext != null)
            {
                b.HasQueryFilter(c => !c.IsDeleted && (_tenantContext.IsTenantFilterIgnored || c.TenantId == _tenantContext.TenantId));
            }
        });
    }
}
