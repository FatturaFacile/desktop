// <copyright file="SwissPayrollViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Swiss payroll module: employees, monthly inputs, payroll runs, salary certificates, reference tables and settings.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SwissPayrollViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="printService">The print service.</param>
/// <param name="fileDialogService">The file dialogs.</param>
internal sealed partial class SwissPayrollViewModel(ISender sender, ILocalizationService localizationService, IReportPrintService printService, IFileDialogService fileDialogService) : ObservableObject
{
    private readonly ILocalizationService _localization = localizationService;
    private readonly IFileDialogService _dialogs = fileDialogService;

    private readonly ISender _sender = sender;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Gets the employees section.
    /// </summary>
    public PayrollEmployeesViewModel Employees { get; } = new(sender, localizationService);

    /// <summary>
    /// Gets the monthly inputs section.
    /// </summary>
    public PayrollInputsViewModel Inputs { get; } = new(sender, localizationService);

    /// <summary>
    /// Gets the payroll runs section.
    /// </summary>
    public PayrollRunsViewModel Runs { get; } = new(sender, localizationService, printService);

    /// <summary>
    /// Gets the salary certificates section.
    /// </summary>
    public PayrollCertificatesViewModel Certificates { get; } = new(sender, localizationService, printService);

    /// <summary>
    /// Gets the reference tables section.
    /// </summary>
    public PayrollTablesViewModel Tables { get; } = new(sender, localizationService, fileDialogService);

    /// <summary>
    /// Gets the settings section.
    /// </summary>
    public PayrollSettingsViewModel Settings { get; } = new(sender, localizationService);

    /// <summary>
    /// Creates the default setup of the company when missing and loads every section.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            _ = await _sender.Send(new EnsureSwissPayrollSetupCommand());
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }

        await Employees.LoadAsync();
        await Inputs.LoadAsync();
        await Runs.LoadAsync();
        await Certificates.LoadAsync();
        await Tables.LoadAsync();
        await Settings.LoadAsync();
    }

    /// <summary>
    /// Saves the yearly salary declaration (AHV, ALV, UVG, KTG, BVG) of the year selected in the certificates.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    private async Task ExportSalaryDeclarationAsync()
    {
        try
        {
            var file = await _sender.Send(new GetSwissSalaryDeclarationQuery(Certificates.Year));
            var path = _dialogs.ShowSaveFile(_localization["Payroll_SalaryDeclaration"], "CSV (*.csv)|*.csv", file.FileName);

            if (path is null)
            {
                return;
            }

            await File.WriteAllBytesAsync(path, file.Content);
            Certificates.StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization["Payroll_FileSaved"], path, file.Summary);
        }
        catch (InvalidOperationException ex)
        {
            Certificates.StatusMessage = ex.Message;
        }
    }
}
