// <copyright file="InvoiceConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Invoicing.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Invoicing.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Invoice"/> entity.
/// </summary>
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        _ = builder.ToTable("invoices");

        _ = builder.HasKey(i => i.Id);
        _ = builder.Property(i => i.Id).ValueGeneratedNever();

        _ = builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(50)
            .IsRequired();

        _ = builder.Property(i => i.Currency)
            .HasMaxLength(3)
            .IsRequired();

        _ = builder.Property(i => i.TotalNet).HasPrecision(18, 2);
        _ = builder.Property(i => i.TotalTax).HasPrecision(18, 2);
        _ = builder.Property(i => i.TotalGross).HasPrecision(18, 2);
        _ = builder.Property(i => i.RoundingAmount).HasPrecision(18, 2);
        _ = builder.Property(i => i.TotalPaid).HasPrecision(18, 2);
        _ = builder.Property(i => i.ExchangeRate).HasPrecision(12, 6);

        _ = builder.Property(i => i.Notes).HasMaxLength(2000);

        _ = builder.Property(i => i.InvoiceType).HasConversion<string>().HasMaxLength(30);
        _ = builder.Property(i => i.PaymentStatus).HasConversion<string>().HasMaxLength(30);
        _ = builder.Property(i => i.Jurisdiction).HasConversion<string>().HasMaxLength(10);

        _ = builder.Property(i => i.RowVersion)
            .IsRowVersion()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        _ = builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();
        _ = builder.HasIndex(i => new { i.TenantId, i.Date });
        _ = builder.HasIndex(i => new { i.TenantId, i.PaymentStatus });

        _ = builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
