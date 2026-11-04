// <copyright file="InvoiceItemRowModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Desktop.Wpf.ViewModels;

/// <summary>
/// Observable model representing an editable row in the invoice creation line item table.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoiceItemRowModel"/> class.
/// </remarks>
/// <param name="parent">The parent invoice detail view model for calculation coordination.</param>
public sealed class InvoiceItemRowModel(InvoiceDetailViewModel parent) : ObservableObject
{
    private readonly InvoiceDetailViewModel _parent = parent;

    private string _description = string.Empty;

    private decimal _quantity = 1;

    private decimal _unitPrice;

    private decimal _taxRate = 8.1m;

    private decimal _discountPercentage;

    private decimal _lineTotal;

    /// <summary>
    /// Gets or sets the line item description.
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// Gets or sets the line item quantity.
    /// </summary>
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets or sets the net unit price.
    /// </summary>
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (SetProperty(ref _unitPrice, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets or sets the VAT percentage rate (default: 8.1% Swiss VAT).
    /// </summary>
    public decimal TaxRate
    {
        get => _taxRate;
        set
        {
            if (SetProperty(ref _taxRate, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets or sets the line discount percentage (0 to 100).
    /// </summary>
    public decimal DiscountPercentage
    {
        get => _discountPercentage;
        set
        {
            if (SetProperty(ref _discountPercentage, value))
            {
                _parent.RecalculateTotals();
            }
        }
    }

    /// <summary>
    /// Gets or sets the calculated net line total.
    /// </summary>
    public decimal LineTotal
    {
        get => _lineTotal;
        set => SetProperty(ref _lineTotal, value);
    }
}
