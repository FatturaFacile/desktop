// <copyright file="FinanceViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Finance;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Finance module: supplier invoices, bank, VAT returns and fixed assets.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FinanceViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="fileDialogService">The file dialogs.</param>
internal sealed class FinanceViewModel(ISender sender, ILocalizationService localizationService, IFileDialogService fileDialogService) : ObservableObject
{
    /// <summary>
    /// Gets the supplier invoices section.
    /// </summary>
    public PurchasesSectionViewModel Purchases { get; } = new(sender, localizationService);

    /// <summary>
    /// Gets the bank section.
    /// </summary>
    public BankSectionViewModel Bank { get; } = new(sender, localizationService, fileDialogService);

    /// <summary>
    /// Gets the VAT returns section.
    /// </summary>
    public VatSectionViewModel Vat { get; } = new(sender, localizationService);

    /// <summary>
    /// Gets the fixed assets section.
    /// </summary>
    public AssetsSectionViewModel Assets { get; } = new(sender, localizationService);

    /// <summary>
    /// Loads every section.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    public async Task LoadAsync()
    {
        await Purchases.LoadAsync();
        await Bank.LoadAsync();
        await Vat.LoadAsync();
        await Assets.LoadAsync();
    }
}
