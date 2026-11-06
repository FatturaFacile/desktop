// <copyright file="VatSectionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Application.Features.Taxation;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Finance;

/// <summary>
/// Periodic VAT returns (Swiss MWST statement, Italian liquidation).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VatSectionViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed partial class VatSectionViewModel(ISender sender, ILocalizationService localizationService) : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private DateTime _from = QuarterStart(DateTime.Today);

    [ObservableProperty]
    private DateTime _to = QuarterStart(DateTime.Today).AddMonths(3).AddDays(-1);

    [ObservableProperty]
    private VatReturn? _selectedReturn;

    /// <summary>
    /// Gets the VAT returns.
    /// </summary>
    public ObservableCollection<VatReturn> Returns { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedReturn?.Id;
        var returns = await Sender.Send(new GetRecordsQuery<VatReturn>());
        Returns.Clear();

        foreach (var vatReturn in returns.OrderByDescending(static r => r.PeriodFrom))
        {
            Returns.Add(vatReturn);
        }

        SelectedReturn = Returns.FirstOrDefault(r => r.Id == selectedId) ?? Returns.FirstOrDefault();
    });

    private static DateTime QuarterStart(DateTime date) => new(date.Year, ((date.Month - 1) / 3 * 3) + 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [RelayCommand]
    private Task CalculateAsync() => RunAsync(
        async () =>
        {
            var id = await Sender.Send(new CalculateVatReturnCommand(From, To));
            await LoadAsync();
            SelectedReturn = Returns.FirstOrDefault(r => r.Id == id);
        },
        "Finance_VatCalculated");

    [RelayCommand]
    private Task FinalizeAsync() => SelectedReturn is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new FinalizeVatReturnCommand(SelectedReturn.Id));
                await LoadAsync();
            },
            "Finance_VatFinalized");
}
