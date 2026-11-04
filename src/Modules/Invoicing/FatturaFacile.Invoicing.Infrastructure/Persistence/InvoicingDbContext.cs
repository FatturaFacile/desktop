using Microsoft.EntityFrameworkCore;
using FatturaFacile.Infrastructure.Common.Audit;
using FatturaFacile.Infrastructure.Common.Outbox;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Entities;
using FatturaFacile.Invoicing.Infrastructure.Configurations;

namespace FatturaFacile.Invoicing.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Invoicing bounded context.
/// </summary>
public class InvoicingDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    public InvoicingDbContext(DbContextOptions<InvoicingDbContext> options, ITenantContext? tenantContext = null) 
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceItemConfiguration());

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox_messages");
            b.HasKey(o => o.Id);
            b.Property(o => o.Type).HasMaxLength(255).IsRequired();
            b.HasIndex(o => o.ProcessedOnUtc);
        });

        modelBuilder.Entity<AuditLogEntry>(b =>
        {
            b.ToTable("audit_logs");
            b.HasKey(a => a.Id);
            b.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
            b.Property(a => a.Action).HasMaxLength(50).IsRequired();
            b.HasIndex(a => new { a.TenantId, a.Timestamp });
        });

        if (_tenantContext != null)
        {
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => 
                !i.IsDeleted && (_tenantContext.IsTenantFilterIgnored || i.TenantId == _tenantContext.TenantId));

            modelBuilder.Entity<InvoiceItem>().HasQueryFilter(i => 
                _tenantContext.IsTenantFilterIgnored || i.TenantId == _tenantContext.TenantId);
        }
    }
}
