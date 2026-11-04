// <copyright file="InvoicingDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Infrastructure.Common.Audit;
using FatturaFacile.Infrastructure.Common.Outbox;
using FatturaFacile.Infrastructure.Common.Tenancy;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Entities;
using FatturaFacile.Invoicing.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Invoicing.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the Invoicing bounded context.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoicingDbContext"/> class.
/// </remarks>
/// <param name="options">The database context options.</param>
/// <param name="tenantContext">The optional ambient tenant context.</param>
public class InvoicingDbContext(DbContextOptions<InvoicingDbContext> options, ITenantContext? tenantContext = null) : DbContext(options)
{
    private const int MaxOutboxMessageTypeLength = 255;
    private const int MaxAuditLogEntityNameLength = 100;
    private const int MaxAuditLogActionLength = 50;

    private readonly ITenantContext? _tenantContext = tenantContext;

    /// <summary>
    /// Gets the database set for invoices.
    /// </summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <summary>
    /// Gets the database set for invoice items.
    /// </summary>
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    /// <summary>
    /// Gets the database set for transactional outbox domain events.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Gets the database set for entity mutation audit logs.
    /// </summary>
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        _ = modelBuilder.ApplyConfiguration(new InvoiceItemConfiguration());

        _ = modelBuilder.Entity<OutboxMessage>(b =>
        {
            _ = b.ToTable("outbox_messages");
            _ = b.HasKey(o => o.Id);
            _ = b.Property(o => o.Type).HasMaxLength(MaxOutboxMessageTypeLength).IsRequired();
            _ = b.HasIndex(o => o.ProcessedOnUtc);
        });

        _ = modelBuilder.Entity<AuditLogEntry>(b =>
        {
            _ = b.ToTable("audit_logs");
            _ = b.HasKey(a => a.Id);
            _ = b.Property(a => a.EntityName).HasMaxLength(MaxAuditLogEntityNameLength).IsRequired();
            _ = b.Property(a => a.Action).HasMaxLength(MaxAuditLogActionLength).IsRequired();
            _ = b.HasIndex(a => new { a.TenantId, a.Timestamp });
        });

        if (_tenantContext != null)
        {
            _ = modelBuilder.Entity<Invoice>().HasQueryFilter(i =>
                !i.IsDeleted && (_tenantContext.IsTenantFilterIgnored || i.TenantId == _tenantContext.TenantId));

            _ = modelBuilder.Entity<InvoiceItem>().HasQueryFilter(i =>
                _tenantContext.IsTenantFilterIgnored || i.TenantId == _tenantContext.TenantId);
        }
    }
}
