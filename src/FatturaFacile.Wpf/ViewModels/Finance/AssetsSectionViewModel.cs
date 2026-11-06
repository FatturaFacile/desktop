// <copyright file="AssetsSectionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Assets;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Features.Assets;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Finance;

/// <summary>
/// Fixed assets with their depreciation plan and the yearly depreciation posting.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AssetsSectionViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed partial class AssetsSectionViewModel(ISender sender, ILocalizationService localizationService) : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private FixedAsset? _selectedAsset;

    [ObservableProperty]
    private FixedAsset _editing = NewAsset();

    [ObservableProperty]
    private int _year = DateTime.Today.Year;

    /// <summary>
    /// Gets the fixed assets.
    /// </summary>
    public ObservableCollection<FixedAsset> Assets { get; } = [];

    /// <summary>
    /// Gets the depreciation plan of the edited asset.
    /// </summary>
    public ObservableCollection<DepreciationPlanRow> Plan { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedAsset?.Id;
        var assets = await Sender.Send(new GetRecordsQuery<FixedAsset>());
        Assets.Clear();

        foreach (var asset in assets.OrderBy(static a => a.Code, StringComparer.Ordinal))
        {
            Assets.Add(asset);
        }

        SelectedAsset = Assets.FirstOrDefault(a => a.Id == selectedId) ?? Assets.FirstOrDefault();
    });

    private static FixedAsset NewAsset() => new() { Id = Guid.Empty, RatePercent = 20m, Method = DepreciationCalculator.StraightLine };

    partial void OnSelectedAssetChanged(FixedAsset? value)
    {
        Editing = value is null ? NewAsset() : RecordCopier.Clone(value);
        RefreshPlan();
    }

    [RelayCommand]
    private void RefreshPlan()
    {
        Plan.Clear();

        foreach (var (year, amount, bookValue) in DepreciationCalculator.Plan(Editing))
        {
            Plan.Add(new DepreciationPlanRow(year, amount, bookValue));
        }
    }

    [RelayCommand]
    private void New()
    {
        SelectedAsset = null;
        Editing = NewAsset();
        Plan.Clear();
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(
        async () =>
        {
            var id = await Sender.Send(new SaveRecordCommand<FixedAsset>(Editing));
            await LoadAsync();
            SelectedAsset = Assets.FirstOrDefault(a => a.Id == id);
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteAsync() => SelectedAsset is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeleteRecordCommand<FixedAsset>(SelectedAsset.Id));
                await LoadAsync();
            },
            "Common_Deleted");

    [RelayCommand]
    private Task PostDepreciationAsync() => RunAsync(async () =>
    {
        var count = await Sender.Send(new PostDepreciationCommand(Year));
        StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Finance_DepreciationPosted"], count, Year);
    });
}
