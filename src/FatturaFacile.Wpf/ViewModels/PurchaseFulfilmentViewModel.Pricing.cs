// <copyright file="PurchaseFulfilmentViewModel.Pricing.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Procurement;
using FatturaFacile.Application.Procurement;

namespace FatturaFacile.Wpf.ViewModels;

internal sealed partial class PurchaseFulfilmentViewModel
{
    [ObservableProperty]
    private string _pricingQuantityText = "1";

    [ObservableProperty]
    private string _pricingPriceText = string.Empty;

    [ObservableProperty]
    private string _pricingCascadeText = string.Empty;

    [ObservableProperty]
    private string _pricingFactorText = string.Empty;

    [ObservableProperty]
    private string _pricingRateText = string.Empty;

    [ObservableProperty]
    private string _pricingTaxText = string.Empty;

    [ObservableProperty]
    private bool _hasPricing;

    [ObservableProperty]
    private string _pricingNote = string.Empty;

    [ObservableProperty]
    private string _effectiveDiscountText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _netUnitPriceText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _lineNetText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _lineTaxText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _lineTotalText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _quantityBaseText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _pricePerBaseText = RebateFormat.NoValue;

    [ObservableProperty]
    private string _companyCurrencyText = RebateFormat.NoValue;

    [RelayCommand]
    private async Task PreviewPricingAsync()
    {
        if (ReadPricingRequest() is not { } request)
        {
            StatusMessage = _localization["Procurement_InvalidNumber"];

            return;
        }

        IsBusy = true;

        try
        {
            var load = await _sender.Send(new PreviewPurchasePricingQuery(request));
            var data = load.Data;
            EffectiveDiscountText = RebateFormat.Percent(data.EffectiveDiscountPercent, Culture);
            NetUnitPriceText = data.NetUnitPrice.ToString("N4", Culture);
            LineNetText = data.LineNetAmount.ToString("N2", Culture);
            LineTaxText = data.LineTaxAmount.ToString("N2", Culture);
            LineTotalText = data.LineTotalAmount.ToString("N2", Culture);
            QuantityBaseText = data.QuantityBase.ToString("0.####", Culture);
            PricePerBaseText = data.NetPricePerBaseUnit.ToString("N4", Culture);
            CompanyCurrencyText = data.LineNetAmountCompanyCurrency.ToString("N2", Culture);
            PricingNote = load.FromCache ? _localization["Procurement_PricingEstimate"] : string.Empty;
            HasPricing = true;
            StatusMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Reads the pricing form; null when a number is missing or out of range.
    private PricingPreviewRequest? ReadPricingRequest()
    {
        var read = ProcurementInput.TryDecimal(PricingQuantityText, Culture, out var quantity)
            & ProcurementInput.TryDecimal(PricingPriceText, Culture, out var price)
            & ProcurementInput.TryDecimal(PricingFactorText, Culture, out var factor)
            & ProcurementInput.TryDecimal(PricingRateText, Culture, out var rate)
            & ProcurementInput.TryDecimal(PricingTaxText, Culture, out var tax);
        if (!read || quantity is not > 0m || price is not >= 0m)
        {
            return null;
        }

        if (factor is <= 0m || rate is <= 0m || tax is < 0m or > 100m)
        {
            return null;
        }

        return PurchasePricingCalculator.TryParseCascade(PricingCascadeText, out var cascade)
            ? new PricingPreviewRequest(quantity.Value, price.Value, cascade, factor, rate, tax)
            : null;
    }
}
