// <copyright file="InvoiceLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents an individual itemized line on an outbound sales invoice, capturing product or service details,
/// unit pricing, quantities, applicable VAT rates, and statutory tax exemption natures.
/// </summary>
public sealed class InvoiceLine : Entity<int>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceLine"/> class with position index, item details,
    /// pricing, and fiscal categorization.
    /// </summary>
    /// <param name="lineNumber">The progressive 1-based sequential line index within the invoice (corresponds to SDI tag NumeroLinea).</param>
    /// <param name="description">The commercial or descriptive textual detail of the goods supplied or services rendered.</param>
    /// <param name="quantity">The billed unit quantity or volume for the line item.</param>
    /// <param name="unitPrice">The agreed net unit price excluding value-added tax.</param>
    /// <param name="vatRate">The applicable VAT percentage rate (e.g. 22.0 for standard Italian VAT, 8.1 for standard Swiss VAT, or 0.0 for exempt/out-of-scope transactions).</param>
    /// <param name="natura">The Italian SDI VAT exemption nature code (e.g., "N1", "N2.1", "N3.1", "N4") if the VAT rate is zero; otherwise, <see langword="null"/>.</param>
    internal InvoiceLine(
        int     lineNumber,
        string  description,
        decimal quantity,
        decimal unitPrice,
        decimal vatRate,
        string? natura)
    {
        Id                  = lineNumber;
        LineNumber          = lineNumber;
        Description         = description;
        Quantity            = quantity;
        UnitPrice           = unitPrice;
        VatRate             = vatRate;
        NaturaExemptionCode = natura;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceLine"/> class for entity framework or serialization.
    /// </summary>
    private InvoiceLine()
    {
    }

    /// <summary>
    /// Gets the progressive 1-based line position number within the invoice (corresponds to SDI tag NumeroLinea).
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the textual description of the goods sold or professional services delivered.
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    /// Gets the quantity of goods or hours/units of service rendered.
    /// </summary>
    public decimal Quantity { get; }

    /// <summary>
    /// Gets the unit price per item or unit of measure, net of VAT and discounts.
    /// </summary>
    public decimal UnitPrice { get; }

    /// <summary>
    /// Gets the VAT rate percentage applied to this line item (e.g., 22.0, 10.0, 8.1, or 0.0).
    /// </summary>
    public decimal VatRate { get; }

    /// <summary>
    /// Gets the statutory Italian tax exemption nature code (element &lt;Natura&gt; in FatturaPA, e.g., N1, N2.2, N3.5, N4, N6) required whenever the VAT rate is zero.
    /// </summary>
    public string? NaturaExemptionCode { get; }

    /// <summary>
    /// Gets the total net amount for the line item, calculated as <see cref="Quantity"/> multiplied
    /// by <see cref="UnitPrice"/> rounded to 2 decimal places using standard midpoint rounding away from zero.
    /// </summary>
    public decimal LineTotal => decimal.Round(
                                              d: Quantity * UnitPrice,
                                              decimals: 2,
                                              mode: MidpointRounding.AwayFromZero);

}
