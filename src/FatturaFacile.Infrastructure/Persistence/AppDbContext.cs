// <copyright file="AppDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Linq.Expressions;
using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Primary Entity Framework Core database context providing automated multi-tenant query filtering,
/// soft-delete lifecycle tracking, and SQLite type conversions.
/// </summary>
public class AppDbContext : DbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    /// <summary>
    /// Gets the ambient tenant identifier used in global query filter evaluations.
    /// </summary>
    public Guid? CurrentTenantId => _currentTenantService?.TenantId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    /// <param name="currentTenantService">The ambient tenant context service.</param>
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    /// <summary>
    /// Gets the database set for tenant organizations.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Gets the database set for user accounts.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the database set for tenant-user membership associations.
    /// </summary>
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    /// <summary>
    /// Gets the database set for client and supplier contacts.
    /// </summary>
    public DbSet<Contact> Contacts => Set<Contact>();

    /// <summary>
    /// Gets the database set for products and services.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets the database set for invoice documents.
    /// </summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <summary>
    /// Gets the database set for invoice line items.
    /// </summary>
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    /// <summary>
    /// Gets the database set for VAT codes and tax rates.
    /// </summary>
    public DbSet<VatCode> VatCodes => Set<VatCode>();

    /// <summary>
    /// Gets the database set for sequential numbering sequences.
    /// </summary>
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();

    /// <summary>
    /// Gets the database set for tenant backup records.
    /// </summary>
    public DbSet<TenantBackup> TenantBackups => Set<TenantBackup>();

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                if (entry.Entity is IMustHaveTenant tenantEntity && tenantEntity.TenantId == Guid.Empty && _currentTenantService.TenantId.HasValue)
                {
                    tenantEntity.TenantId = _currentTenantService.TenantId.Value;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = now;
                entry.Entity.UpdatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global SQLite Type Converters
        var decimalConverter = new ValueConverter<decimal, string>(
            v => v.ToString("0.####", CultureInfo.InvariantCulture),
            v => decimal.Parse(v, CultureInfo.InvariantCulture));

        var nullableDecimalConverter = new ValueConverter<decimal?, string?>(
            v => v.HasValue ? v.Value.ToString("0.####", CultureInfo.InvariantCulture) : null,
            v => !string.IsNullOrEmpty(v) ? decimal.Parse(v, CultureInfo.InvariantCulture) : null);

        var dateTimeConverter = new ValueConverter<DateTime, string>(
            v => v.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture),
            v => DateTime.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, string?>(
            v => v.HasValue ? v.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture) : null,
            v => !string.IsNullOrEmpty(v) ? DateTime.Parse(v, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : null);

        var guidConverter = new ValueConverter<Guid, string>(
            v => v.ToString("D"),
            v => Guid.Parse(v));

        var nullableGuidConverter = new ValueConverter<Guid?, string?>(
            v => v.HasValue ? v.Value.ToString("D") : null,
            v => !string.IsNullOrEmpty(v) ? Guid.Parse(v) : null);

        // Configure entities and properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // Apply SQLite type conversions
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal))
                {
                    property.SetValueConverter(decimalConverter);
                }
                else if (property.ClrType == typeof(decimal?))
                {
                    property.SetValueConverter(nullableDecimalConverter);
                }
                else if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
                else if (property.ClrType == typeof(Guid))
                {
                    property.SetValueConverter(guidConverter);
                }
                else if (property.ClrType == typeof(Guid?))
                {
                    property.SetValueConverter(nullableGuidConverter);
                }
            }

            // Apply Multi-Tenancy and Soft Delete Query Filters
            _ = modelBuilder.Entity<Contact>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == CurrentTenantId);
            _ = modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == CurrentTenantId);
            _ = modelBuilder.Entity<InvoiceItem>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            _ = modelBuilder.Entity<VatCode>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            _ = modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == CurrentTenantId);
            _ = modelBuilder.Entity<DocumentSequence>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            _ = modelBuilder.Entity<TenantBackup>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
            _ = modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
            _ = modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        }

        // Entity-specific configurations
        _ = modelBuilder.Entity<TenantUser>()
            .HasOne(tu => tu.Tenant)
            .WithMany(t => t.TenantUsers)
            .HasForeignKey(tu => tu.TenantId);

        _ = modelBuilder.Entity<TenantUser>()
            .HasOne(tu => tu.User)
            .WithMany(u => u.TenantUsers)
            .HasForeignKey(tu => tu.UserId);

        _ = modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Contact)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        _ = modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Items)
            .WithOne(it => it.Invoice)
            .HasForeignKey(it => it.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = modelBuilder.Entity<DocumentSequence>()
            .HasIndex(ds => new { ds.TenantId, ds.DocumentType, ds.FiscalYear })
            .IsUnique();
    }
}
