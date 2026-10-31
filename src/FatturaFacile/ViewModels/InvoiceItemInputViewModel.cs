// <copyright file="InvoiceItemInputViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Presentation view model representing an editable row in the invoice line item creation grid.
/// </summary>
public class InvoiceItemInputViewModel : ObservableObject
{
    private readonly Action _onChanged;

    /// <summary>
    /// Gets or sets the optional linked catalog product identifier.
    /// </summary>
    public Guid? ProductId { get; set; }

    private string _description = "Service / Product";

    /// <summary>
    /// Gets or sets the textual description of the billed good or service.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            _ = SetProperty(ref _description, value);
            _onChanged();
        }
    }

    private decimal _quantity = 1.0m;

    /// <summary>
    /// Gets or sets the quantity of units for this line.
    /// </summary>
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            _ = SetProperty(ref _quantity, value);
            _onChanged();
        }
    }

    private decimal _unitPrice = 100.00m;

    /// <summary>
    /// Gets or sets the unit price before line discount and VAT.
    /// </summary>
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            _ = SetProperty(ref _unitPrice, value);
            _onChanged();
        }
    }

    private decimal _taxRate = 8.1m;

    /// <summary>
    /// Gets or sets the applicable VAT tax rate percentage.
    /// </summary>
    public decimal TaxRate
    {
        get => _taxRate;
        set
        {
            _ = SetProperty(ref _taxRate, value);
            _onChanged();
        }
    }

    private decimal _discountPercentage = 0.0m;

    /// <summary>
    /// Gets or sets the discount percentage applied to this line item.
    /// </summary>
    public decimal DiscountPercentage
    {
        get => _discountPercentage;
        set
        {
            _ = SetProperty(ref _discountPercentage, value);
            _onChanged();
        }
    }

    private decimal _lineTotal;

    /// <summary>
    /// Gets or sets the computed line net subtotal after discounts.
    /// </summary>
    public decimal LineTotal
    {
        get => _lineTotal;
        set => SetProperty(ref _lineTotal, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceItemInputViewModel"/> class.
    /// </summary>
    /// <param name="onChanged">Callback executed when any pricing or quantity parameter changes.</param>
    public InvoiceItemInputViewModel(Action onChanged)
    {
        _onChanged = onChanged;
    }
}
