// <copyright file="PayrollTablesViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Reference tables: withholding tax tariffs (ESTV import), family allowance amounts and wage types.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PayrollTablesViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="fileDialogService">The file dialogs.</param>
internal sealed partial class PayrollTablesViewModel(ISender sender, ILocalizationService localizationService, IFileDialogService fileDialogService) : PayrollSectionViewModel(sender, localizationService)
{
    private readonly IFileDialogService _fileDialogService = fileDialogService;

    [ObservableProperty]
    private FamilyAllowanceRate? _selectedRate;

    [ObservableProperty]
    private FamilyAllowanceRate _editingRate = new();

    [ObservableProperty]
    private PayrollWageType? _selectedWageType;

    [ObservableProperty]
    private PayrollWageType _editingWageType = new();

    /// <summary>
    /// Gets the imported tariffs.
    /// </summary>
    public ObservableCollection<WithholdingTaxTariffSummaryDto> Tariffs { get; } = [];

    /// <summary>
    /// Gets the family allowance amounts.
    /// </summary>
    public ObservableCollection<FamilyAllowanceRate> Rates { get; } = [];

    /// <summary>
    /// Gets the wage types.
    /// </summary>
    public ObservableCollection<PayrollWageType> WageTypes { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var tariffs = await Sender.Send(new GetWithholdingTaxTariffSummaryQuery());
        var rates = await Sender.Send(new GetPayrollRecordsQuery<FamilyAllowanceRate>());
        var wageTypes = await Sender.Send(new GetPayrollRecordsQuery<PayrollWageType>());

        Tariffs.Clear();
        Rates.Clear();
        WageTypes.Clear();

        foreach (var tariff in tariffs)
        {
            Tariffs.Add(tariff);
        }

        foreach (var rate in rates.OrderBy(static r => r.Canton, StringComparer.Ordinal).ThenByDescending(static r => r.ValidFrom))
        {
            Rates.Add(rate);
        }

        foreach (var wageType in wageTypes.OrderBy(static w => w.Code, StringComparer.Ordinal))
        {
            WageTypes.Add(wageType);
        }
    });

    partial void OnSelectedRateChanged(FamilyAllowanceRate? value) =>
        EditingRate = value is null ? new FamilyAllowanceRate() : RecordCopier.Clone(value);

    partial void OnSelectedWageTypeChanged(PayrollWageType? value) =>
        EditingWageType = value is null ? new PayrollWageType() : RecordCopier.Clone(value);

    [RelayCommand]
    private Task ImportTariffsAsync()
    {
        var path = _fileDialogService.ShowOpenFile(Localization["Payroll_ImportTariffs"], "ESTV (*.txt)|*.txt|*.*|*.*");

        return path is null
            ? Task.CompletedTask
            : RunAsync(async () =>
            {
                var content = await File.ReadAllTextAsync(path, Encoding.Latin1);
                var count = await Sender.Send(new ImportWithholdingTaxTariffsCommand(content));
                await LoadAsync();
                StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Payroll_TariffsImported"], count);
            });
    }

    [RelayCommand]
    private void NewRate()
    {
        SelectedRate = null;
        EditingRate = new FamilyAllowanceRate { Id = Guid.Empty, ValidFrom = new DateTime(DateTime.Today.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) };
    }

    [RelayCommand]
    private Task SaveRateAsync() => RunAsync(
        async () =>
        {
            _ = await Sender.Send(new SavePayrollRecordCommand<FamilyAllowanceRate>(EditingRate));
            await LoadAsync();
        },
        "Common_Saved");

    [RelayCommand]
    private void NewWageType()
    {
        SelectedWageType = null;
        EditingWageType = new PayrollWageType { Id = Guid.Empty, Kind = "Earning", IsActive = true };
    }

    [RelayCommand]
    private Task SaveWageTypeAsync() => RunAsync(
        async () =>
        {
            _ = await Sender.Send(new SavePayrollRecordCommand<PayrollWageType>(EditingWageType));
            await LoadAsync();
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteWageTypeAsync() => SelectedWageType is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeletePayrollRecordCommand<PayrollWageType>(SelectedWageType.Id));
                await LoadAsync();
            },
            "Common_Deleted");
}
