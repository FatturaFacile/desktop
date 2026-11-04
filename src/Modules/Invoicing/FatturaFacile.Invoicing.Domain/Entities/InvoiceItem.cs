using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Invoicing.Domain.Entities;

public sealed class InvoiceItem : Entity<Guid>, IHasTenant
{
    public Guid TenantId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public Guid? ProductId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal LineTotal { get; private set; }
    public decimal TaxAmount { get; private set; }

    private InvoiceItem() { }

    public static InvoiceItem Create(
        Guid tenantId,
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        decimal discountPercentage = 0m)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Item description cannot be empty.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Tax rate must be between 0 and 100.", nameof(taxRate));
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(discountPercentage));

        var subtotal = quantity * unitPrice;
        if (discountPercentage > 0)
        {
            subtotal -= (subtotal * (discountPercentage / 100m));
        }
        var lineTotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero);
        var taxAmount = decimal.Round(lineTotal * (taxRate / 100m), 2, MidpointRounding.AwayFromZero);

        return new InvoiceItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = productId,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            DiscountPercentage = discountPercentage,
            LineTotal = lineTotal,
            TaxAmount = taxAmount
        };
    }
}
