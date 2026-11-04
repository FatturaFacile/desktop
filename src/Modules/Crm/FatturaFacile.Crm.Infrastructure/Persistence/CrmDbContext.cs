// <copyright file="CrmDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Crm.Domain.Aggregates;
using FatturaFacile.Infrastructure.Common.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Crm.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the CRM bounded context.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CrmDbContext"/> class.
/// </remarks>
/// <param name="options">The database context options.</param>
/// <param name="tenantContext">The optional ambient tenant context accessor.</param>
public class CrmDbContext(DbContextOptions<CrmDbContext> options, ITenantContext? tenantContext = null) : DbContext(options)
{
    private readonly ITenantContext? _tenantContext = tenantContext;

    /// <summary>
    /// Gets the database set for customer and supplier contacts.
    /// </summary>
    public DbSet<Contact> Contacts => Set<Contact>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<Contact>(b =>
        {
            _ = b.ToTable("contacts");
            _ = b.HasKey(c => c.Id);
            _ = b.Property(c => c.Id).ValueGeneratedNever();
            _ = b.Property(c => c.Name).HasMaxLength(255).IsRequired();
            _ = b.Property(c => c.TradeName).HasMaxLength(255);
            _ = b.Property(c => c.VatNumber).HasMaxLength(50);
            _ = b.Property(c => c.TaxCode).HasMaxLength(50);
            _ = b.Property(c => c.Email).HasMaxLength(255);
            _ = b.Property(c => c.Phone).HasMaxLength(50);
            _ = b.Property(c => c.Pec).HasMaxLength(255);
            _ = b.Property(c => c.SdiCode).HasMaxLength(20);
            _ = b.Property(c => c.CountryCode).HasMaxLength(2).IsRequired();
            _ = b.Property(c => c.ContactType).HasConversion<string>().HasMaxLength(30);

            _ = b.HasIndex(c => new { c.TenantId, c.Name });
            _ = b.HasIndex(c => new { c.TenantId, c.VatNumber });

            if (_tenantContext != null)
            {
                _ = b.HasQueryFilter(c => !c.IsDeleted && (_tenantContext.IsTenantFilterIgnored || c.TenantId == _tenantContext.TenantId));
            }
        });
    }
}
