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
internal sealed partial class InvoiceItemRowModel(InvoiceDetailViewModel parent) : ObservableObject
{
    private readonly InvoiceDetailViewModel _parent = parent;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private decimal _quantity = 1;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private decimal _taxRate = 8.1m;

    [ObservableProperty]
    private decimal _discountPercentage;

    [ObservableProperty]
    private decimal _lineTotal;

    partial void OnQuantityChanged(decimal value) => _parent.RecalculateTotals();

    partial void OnUnitPriceChanged(decimal value) => _parent.RecalculateTotals();

    partial void OnTaxRateChanged(decimal value) => _parent.RecalculateTotals();

    partial void OnDiscountPercentageChanged(decimal value) => _parent.RecalculateTotals();
}
