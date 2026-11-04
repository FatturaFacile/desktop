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
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal Quantity { get; set; } = 1;

    [ObservableProperty]
    public partial decimal UnitPrice { get; set; }

    [ObservableProperty]
    public partial decimal TaxRate { get; set; } = 8.1m;

    [ObservableProperty]
    public partial decimal DiscountPercentage { get; set; }

    [ObservableProperty]
    public partial decimal LineTotal { get; set; }

    partial void OnQuantityChanged(decimal value) => _parent.RecalculateTotals();

    partial void OnUnitPriceChanged(decimal value) => _parent.RecalculateTotals();

    partial void OnTaxRateChanged(decimal value) => _parent.RecalculateTotals();

    partial void OnDiscountPercentageChanged(decimal value) => _parent.RecalculateTotals();
}
