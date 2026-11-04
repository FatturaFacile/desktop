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
public class CrmDbContext(
    DbContextOptions<CrmDbContext> options,
    ITenantContext? tenantContext = null)
    : DbContext(options)
{
    private const int NameMaxLength = 255;
    private const int TradeNameMaxLength = 255;
    private const int VatNumberMaxLength = 50;
    private const int TaxCodeMaxLength = 50;
    private const int EmailMaxLength = 255;
    private const int PhoneMaxLength = 50;
    private const int PecMaxLength = 255;
    private const int SdiCodeMaxLength = 20;
    private const int CountryCodeMaxLength = 2;
    private const int ContactTypeMaxLength = 30;

    private readonly ITenantContext? _tenantContext = tenantContext;

    /// <summary>
    /// Gets the database set for customer and supplier contacts.
    /// </summary>
    public DbSet<Contact> Contacts => Set<Contact>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<Contact>(ConfigureContact);
    }

    private void ConfigureContact(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Contact> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.ToTable("contacts");

        _ = builder.HasKey(c => c.Id);

        _ = builder.Property(c => c.Id)
            .ValueGeneratedNever();

        _ = builder.Property(c => c.Name)
            .HasMaxLength(NameMaxLength)
            .IsRequired();

        _ = builder.Property(c => c.TradeName)
            .HasMaxLength(TradeNameMaxLength);

        _ = builder.Property(c => c.VatNumber)
            .HasMaxLength(VatNumberMaxLength);

        _ = builder.Property(c => c.TaxCode)
            .HasMaxLength(TaxCodeMaxLength);

        _ = builder.Property(c => c.Email)
            .HasMaxLength(EmailMaxLength);

        _ = builder.Property(c => c.Phone)
            .HasMaxLength(PhoneMaxLength);

        _ = builder.Property(c => c.Pec)
            .HasMaxLength(PecMaxLength);

        _ = builder.Property(c => c.SdiCode)
            .HasMaxLength(SdiCodeMaxLength);

        _ = builder.Property(c => c.CountryCode)
            .HasMaxLength(CountryCodeMaxLength)
            .IsRequired();

        _ = builder.Property(c => c.ContactType)
            .HasConversion<string>()
            .HasMaxLength(ContactTypeMaxLength);

        _ = builder.HasIndex(c => new { c.TenantId, c.Name });

        _ = builder.HasIndex(c => new { c.TenantId, c.VatNumber });

        if (_tenantContext is not null)
        {
            _ = builder.HasQueryFilter(c =>
                !c.IsDeleted &&
                (_tenantContext.IsTenantFilterIgnored ||
                 c.TenantId == _tenantContext.TenantId));
        }
    }
}
