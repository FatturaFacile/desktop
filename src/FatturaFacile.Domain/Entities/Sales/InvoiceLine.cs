using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents invoice line.
/// </summary>
public sealed class InvoiceLine : Entity<int>
{
    /// <summary>
    /// Gets the line number.
    /// </summary>
    public int LineNumber { get; }

/// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; } = string.Empty;

/// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal Quantity { get; }

/// <summary>
    /// Gets the unit price.
    /// </summary>
    public decimal UnitPrice { get; }

/// <summary>
    /// Gets the e.g. 22.0.
    /// </summary>
    public decimal VatRate { get; }

/// <summary>
    /// Gets the e.g. N1, N2.1, N3.1, N4.
    /// </summary>
    public string? NaturaExemptionCode { get; }

/// <summary>
    /// Gets the line total.
    /// </summary>
    public decimal LineTotal => decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

/// <summary>
    /// Initializes a new instance of the <see cref="InvoiceLine"/> class.
    /// </summary>
    /// <param name="lineNumber">The line number.</param>
    /// <param name="description">The description.</param>
    /// <param name="quantity">The quantity.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <param name="vatRate">The vat rate.</param>
    /// <param name="natura">The natura.</param>
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
