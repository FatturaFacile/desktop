namespace FatturaFacile.Invoicing.Application.DTOs;

public sealed record InvoiceDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid ContactId { get; init; }
    public string InvoiceType { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public DateOnly? DueDate { get; init; }
    public string Currency { get; init; } = "EUR";
    public decimal TotalNet { get; init; }
    public decimal TotalTax { get; init; }
    public decimal TotalGross { get; init; }
    public decimal RoundingAmount { get; init; }
    public decimal TotalPaid { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string Jurisdiction { get; init; } = string.Empty;
    public bool SplitPayment { get; init; }
    public bool ReverseCharge { get; init; }
    public string? Notes { get; init; }
    public List<InvoiceItemDto> Items { get; init; } = [];
}

public sealed record InvoiceItemDto
{
    public Guid Id { get; init; }
    public Guid? ProductId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }
    public decimal DiscountPercentage { get; init; }
    public decimal LineTotal { get; init; }
    public decimal TaxAmount { get; init; }
}

public sealed record CreateInvoiceItemInput(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal DiscountPercentage = 0m);
