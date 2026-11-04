// <copyright file="InvoiceListItemModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Display projection model representing an invoice item in the list view data grid.
/// </summary>
public sealed class InvoiceListItemModel
{
    /// <summary>
    /// Gets the unique invoice identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the invoice reference number.
    /// </summary>
    public string InvoiceNumber { get; init; } = string.Empty;

    /// <summary>
    /// Gets the invoice issue date.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Gets the payment due date.
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the customer contact identifier.
    /// </summary>
    public Guid ContactId { get; init; }

    /// <summary>
    /// Gets the counterparty customer display name.
    /// </summary>
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the net taxable amount.
    /// </summary>
    public decimal TotalNet { get; init; }

    /// <summary>
    /// Gets the tax amount.
    /// </summary>
    public decimal TotalTax { get; init; }

    /// <summary>
    /// Gets the Swiss rounding adjustment amount.
    /// </summary>
    public decimal RoundingAmount { get; init; }

    /// <summary>
    /// Gets the gross total invoice amount.
    /// </summary>
    public decimal TotalGross { get; init; }

    /// <summary>
    /// Gets the 3-letter currency code.
    /// </summary>
    public string Currency { get; init; } = "CHF";

    /// <summary>
    /// Gets the payment status description string.
    /// </summary>
    public string PaymentStatus { get; init; } = "Unpaid";

    /// <summary>
    /// Displays Swiss CHF amounts formatted according to Swiss accounting guidelines.
    /// </summary>
    public string FormattedTotalGross => $"{Currency} {TotalGross:N2}";
}
