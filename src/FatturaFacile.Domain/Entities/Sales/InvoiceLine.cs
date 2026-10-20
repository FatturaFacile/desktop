using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Sales;

public sealed class InvoiceLine : Entity<int>
{
    public int LineNumber { get; }
    public string Description { get; } = string.Empty;

    public decimal Quantity { get; }

    public decimal UnitPrice { get; }

    /// <summary>
    /// e.g. 22.0
    /// </summary>
    public decimal VatRate { get; }

    /// <summary>
    /// e.g. N1, N2.1, N3.1, N4
    /// </summary>
    public string? NaturaExemptionCode { get; }
    public decimal LineTotal => decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

    internal InvoiceLine(int lineNumber, string description, decimal quantity, decimal unitPrice, decimal vatRate, string? natura)
    {
        Id = lineNumber;
        LineNumber = lineNumber;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        VatRate = vatRate;
        NaturaExemptionCode = natura;
    }

    private InvoiceLine()
    {
    }
}
