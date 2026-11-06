// <copyright file="ItalianPayrollViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Italian;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Italian payroll: monthly runs, employees, collective agreements (CCNL), INPS settings and filings (F24, LUL, UniEmens, CU).
/// </summary>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="printService">The print service.</param>
/// <param name="fileDialogService">The file dialogs.</param>
internal sealed partial class ItalianPayrollViewModel(ISender sender, ILocalizationService localizationService, IReportPrintService printService, IFileDialogService fileDialogService)
    : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private HrEmployee? _selectedEmployee;

    [ObservableProperty]
    private HrEmployee _editingEmployee = new();

    [ObservableProperty]
    private CollectiveAgreement? _selectedAgreement;

    [ObservableProperty]
    private CollectiveAgreement _editingAgreement = new();

    [ObservableProperty]
    private CollectiveAgreementLevel? _selectedLevel;

    [ObservableProperty]
    private CollectiveAgreementLevel _newLevel = new();

    [ObservableProperty]
    private ItalianPayrollSettings _settings = new();

    [ObservableProperty]
    private int _filingYear = DateTime.Today.Year;

    [ObservableProperty]
    private int _filingMonth = DateTime.Today.Month;

    /// <summary>
    /// Gets the monthly runs.
    /// </summary>
    public PayrollRunsViewModel Runs { get; } = new(sender, localizationService, printService);

    /// <summary>
    /// Gets the employees.
    /// </summary>
    public ObservableCollection<HrEmployee> Employees { get; } = [];

    /// <summary>
    /// Gets the collective agreements.
    /// </summary>
    public ObservableCollection<CollectiveAgreement> Agreements { get; } = [];

    /// <summary>
    /// Gets the levels of the selected agreement.
    /// </summary>
    public ObservableCollection<CollectiveAgreementLevel> Levels { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        await Runs.LoadAsync();
        var employeeId = SelectedEmployee?.Id;
        var agreementId = SelectedAgreement?.Id;
        Replace(Employees, (await Sender.Send(new GetPayrollRecordsQuery<HrEmployee>())).OrderBy(static e => e.LastName, StringComparer.CurrentCulture));
        Replace(Agreements, (await Sender.Send(new GetPayrollRecordsQuery<CollectiveAgreement>())).OrderBy(static a => a.Code, StringComparer.Ordinal));
        Settings = (await Sender.Send(new GetPayrollRecordsQuery<ItalianPayrollSettings>())).FirstOrDefault() ?? new ItalianPayrollSettings();
        SelectedEmployee = Employees.FirstOrDefault(e => e.Id == employeeId) ?? Employees.FirstOrDefault();
        SelectedAgreement = Agreements.FirstOrDefault(a => a.Id == agreementId) ?? Agreements.FirstOrDefault();
    });

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();

        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    partial void OnSelectedEmployeeChanged(HrEmployee? value) => EditingEmployee = value is null ? new HrEmployee() : RecordCopier.Clone(value);

    partial void OnSelectedAgreementChanged(CollectiveAgreement? value)
    {
        EditingAgreement = value is null ? new CollectiveAgreement() : RecordCopier.Clone(value);
        _ = LoadLevelsAsync();
    }

    [RelayCommand]
    private Task SaveEmployeeAsync() => RunAsync(
        async () =>
        {
            _ = await Sender.Send(new SavePayrollRecordCommand<HrEmployee>(EditingEmployee));
            await LoadAsync();
        },
        "Common_Saved");

    [RelayCommand]
    private void NewAgreement() => SelectedAgreement = null;

    [RelayCommand]
    private Task SaveAgreementAsync() => RunAsync(
        async () =>
        {
            _ = await Sender.Send(new SavePayrollRecordCommand<CollectiveAgreement>(EditingAgreement));
            await LoadAsync();
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteAgreementAsync() => SelectedAgreement is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeletePayrollRecordCommand<CollectiveAgreement>(SelectedAgreement.Id));
                await LoadAsync();
            },
            "Common_Deleted");

    [RelayCommand]
    private Task AddLevelAsync() => SelectedAgreement is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            NewLevel.AgreementId = SelectedAgreement.Id;
            _ = await Sender.Send(new SavePayrollRecordCommand<CollectiveAgreementLevel>(NewLevel));
            NewLevel = new CollectiveAgreementLevel();
            await LoadLevelsAsync();
        });

    [RelayCommand]
    private Task RemoveLevelAsync() => SelectedLevel is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            _ = await Sender.Send(new DeletePayrollRecordCommand<CollectiveAgreementLevel>(SelectedLevel.Id));
            await LoadLevelsAsync();
        });

    [RelayCommand]
    private Task SaveSettingsAsync() => RunAsync(
        async () =>
        {
            _ = await Sender.Send(new SavePayrollRecordCommand<ItalianPayrollSettings>(Settings));
            await LoadAsync();
        },
        "Common_Saved");

    [RelayCommand]
    private Task ExportF24Async() => RunAsync(async () => await SaveFileAsync(await Sender.Send(new GetPayrollF24Query(FilingYear, FilingMonth))));

    [RelayCommand]
    private Task ExportUniEmensAsync() => RunAsync(async () => await SaveFileAsync(await Sender.Send(new GetUniEmensDataQuery(FilingYear, FilingMonth))));

    [RelayCommand]
    private Task ExportCuAsync() => RunAsync(async () => await SaveFileAsync(await Sender.Send(new GetCuDataQuery(FilingYear))));

    [RelayCommand]
    private Task PrintLabourBookAsync() => Runs.SelectedRun is not { } run
        ? Task.CompletedTask
        : RunAsync(async () => _ = await printService.PrintAsync(ReportDocumentType.LabourBook, run.Id));

    private async Task SaveFileAsync(GeneratedFile file)
    {
        var path = fileDialogService.ShowSaveFile(Localization["Common_Save"], "CSV (*.csv)|*.csv", file.FileName);

        if (path is null)
        {
            return;
        }

        await File.WriteAllBytesAsync(path, file.Content);
        StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Payroll_FileSaved"], path, file.Summary);
    }

    private async Task LoadLevelsAsync()
    {
        var agreementId = SelectedAgreement?.Id;
        Levels.Clear();

        if (agreementId is null)
        {
            return;
        }

        foreach (var level in (await Sender.Send(new GetPayrollRecordsQuery<CollectiveAgreementLevel>(l => l.AgreementId == agreementId))).OrderBy(static l => l.Level, StringComparer.Ordinal))
        {
            Levels.Add(level);
        }
    }
}
