// <copyright file="InvoiceRowViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Presentation model representing an invoice summary row in the invoice listing data grid,
/// featuring pre-formatted currency amounts, dates, and status badge color themes.
/// </summary>
public class InvoiceRowViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the unique identifier of the invoice.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant workspace identifier to which this invoice belongs.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the sequential document display number (e.g. "INV-2026-0001").
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the invoice was issued.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the payment expiration date.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Gets or sets the customer contact display name or organization name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ISO currency code (e.g. "CHF", "EUR").
    /// </summary>
    public string Currency { get; set; } = "CHF";

    /// <summary>
    /// Gets or sets the total net subtotal before VAT.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the total VAT amount across all invoice line items.
    /// </summary>
    public decimal TotalTax { get; set; }

    /// <summary>
    /// Gets or sets the final gross total payable.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the current settlement state (e.g. "Unpaid", "Paid", "Overdue", "PartiallyPaid").
    /// </summary>
    public string PaymentStatus { get; set; } = "Unpaid";

    /// <summary>
    /// Gets or sets the total number of line items billed.
    /// </summary>
    public int ItemsCount { get; set; }

    /// <summary>
    /// Gets the localized string representation of the issue date ("dd.MM.yyyy").
    /// </summary>
    public string FormattedDate => Date.ToString("dd.MM.yyyy");

    /// <summary>
    /// Gets the localized string representation of the due date ("dd.MM.yyyy").
    /// </summary>
    public string FormattedDueDate => DueDate.ToString("dd.MM.yyyy");

    /// <summary>
    /// Gets the formatted gross total string including the currency code.
    /// </summary>
    public string FormattedTotalGross => $"{Currency} {TotalGross:N2}";

    /// <summary>
    /// Gets the formatted net subtotal string including the currency code.
    /// </summary>
    public string FormattedTotalNet => $"{Currency} {TotalNet:N2}";

    /// <summary>
    /// Gets the formatted tax total string including the currency code.
    /// </summary>
    public string FormattedTotalTax => $"{Currency} {TotalTax:N2}";

    /// <summary>
    /// Gets the hexadecimal foreground color for the payment status pill badge.
    /// </summary>
    public string StatusBadgeColor => PaymentStatus.ToLowerInvariant() switch
    {
        "paid" => "#2E7D32",          // Dark Green
        "overdue" => "#C62828",       // Red
        "partiallypaid" => "#EF6C00", // Orange
        _ => "#0288D1"                // Blue
    };

    /// <summary>
    /// Gets the hexadecimal background color for the payment status pill badge.
    /// </summary>
    public string StatusBadgeBackground => PaymentStatus.ToLowerInvariant() switch
    {
        "paid" => "#E8F5E9",
        "overdue" => "#FFEBEE",
        "partiallypaid" => "#FFF3E0",
        _ => "#E1F5FE"
    };
}
