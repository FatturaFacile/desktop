// <copyright file="InvoiceConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text;
using FatturaFacile.Invoicing.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FatturaFacile.Invoicing.Infrastructure.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Invoice"/> entity.
/// </summary>
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    private const int MaxInvoiceNumberLength = 50;
    private const int CurrencyIsoLength = 3;
    private const int MonetaryPrecision = 18;
    private const int MonetaryScale = 2;
    private const int ExchangeRatePrecision = 12;
    private const int ExchangeRateScale = 6;
    private const int MaxNotesLength = 2000;
    private const int MaxTypeLength = 30;
    private const int MaxJurisdictionLength = 10;

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.ToTable("invoices");

        _ = builder.HasKey(i => i.Id);
        _ = builder.Property(i => i.Id).ValueGeneratedNever();

        _ = builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(MaxInvoiceNumberLength)
            .IsRequired();

        _ = builder.Property(i => i.Currency)
            .HasMaxLength(CurrencyIsoLength)
            .IsRequired();

        _ = builder.Property(i => i.TotalNet).HasPrecision(MonetaryPrecision, MonetaryScale);
        _ = builder.Property(i => i.TotalTax).HasPrecision(MonetaryPrecision, MonetaryScale);
        _ = builder.Property(i => i.TotalGross).HasPrecision(MonetaryPrecision, MonetaryScale);
        _ = builder.Property(i => i.RoundingAmount).HasPrecision(MonetaryPrecision, MonetaryScale);
        _ = builder.Property(i => i.TotalPaid).HasPrecision(MonetaryPrecision, MonetaryScale);
        _ = builder.Property(i => i.ExchangeRate).HasPrecision(ExchangeRatePrecision, ExchangeRateScale);

        _ = builder.Property(i => i.Notes).HasMaxLength(MaxNotesLength);

        _ = builder.Property(i => i.InvoiceType).HasConversion<string>().HasMaxLength(MaxTypeLength);
        _ = builder.Property(i => i.PaymentStatus).HasConversion<string>().HasMaxLength(MaxTypeLength);
        _ = builder.Property(i => i.Jurisdiction).HasConversion<string>().HasMaxLength(MaxJurisdictionLength);

        _ = builder.Property(i => i.IsDeleted);
        _ = builder.Property(i => i.DeletedAt);
        _ = builder.Property(i => i.CreatedAt);
        _ = builder.Property(i => i.CreatedBy);
        _ = builder.Property(i => i.UpdatedAt);
        _ = builder.Property(i => i.UpdatedBy);

        _ = builder.Property(i => i.RowVersion)
            .HasConversion(
                v => Encoding.UTF8.GetString(v.ToArray()),
                v => Encoding.UTF8.GetBytes(v))
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
