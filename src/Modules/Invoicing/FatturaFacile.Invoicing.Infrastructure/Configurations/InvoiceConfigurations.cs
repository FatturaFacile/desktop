using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FatturaFacile.Invoicing.Domain.Aggregates;
using FatturaFacile.Invoicing.Domain.Entities;

namespace FatturaFacile.Invoicing.Infrastructure.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(i => i.TotalNet).HasPrecision(18, 2);
        builder.Property(i => i.TotalTax).HasPrecision(18, 2);
        builder.Property(i => i.TotalGross).HasPrecision(18, 2);
        builder.Property(i => i.RoundingAmount).HasPrecision(18, 2);
        builder.Property(i => i.TotalPaid).HasPrecision(18, 2);
        builder.Property(i => i.ExchangeRate).HasPrecision(12, 6);

        builder.Property(i => i.Notes).HasMaxLength(2000);

        builder.Property(i => i.InvoiceType).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.PaymentStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.Jurisdiction).HasConversion<string>().HasMaxLength(10);

        builder.Property(i => i.RowVersion)
            .IsRowVersion()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.Date });
        builder.HasIndex(i => new { i.TenantId, i.PaymentStatus });

        builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(i => i.Quantity).HasPrecision(14, 4);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 4);
        builder.Property(i => i.TaxRate).HasPrecision(6, 2);
        builder.Property(i => i.DiscountPercentage).HasPrecision(6, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);

        builder.HasIndex(i => new { i.TenantId, i.InvoiceId });
    }
}
