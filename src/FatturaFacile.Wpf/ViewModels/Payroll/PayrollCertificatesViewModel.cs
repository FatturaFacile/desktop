// <copyright file="PayrollCertificatesViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Yearly salary certificates (form 11) computed from the closed payroll runs.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PayrollCertificatesViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="printService">The print service.</param>
internal sealed partial class PayrollCertificatesViewModel(ISender sender, ILocalizationService localizationService, IReportPrintService printService) : PayrollSectionViewModel(sender, localizationService)
{
    private readonly IReportPrintService _printService = printService;

    [ObservableProperty]
    private int _year = DateTime.Today.Year;

    [ObservableProperty]
    private SalaryCertificateRow? _selectedRow;

    /// <summary>
    /// Gets the certificates of the year.
    /// </summary>
    public ObservableCollection<SalaryCertificateRow> Rows { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var year = Year;
        var certificates = await Sender.Send(new GetPayrollRecordsQuery<SalaryCertificate>(c => c.Year == year));
        var employees = await Sender.Send(new GetPayrollRecordsQuery<HrEmployee>());
        var names = employees.ToDictionary(static e => e.Id, static e => e.FullName);
        Rows.Clear();

        foreach (var certificate in certificates)
        {
            Rows.Add(new SalaryCertificateRow(certificate, names.GetValueOrDefault(certificate.EmployeeId) ?? "?"));
        }
    });

    partial void OnYearChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    private Task GenerateAsync() => RunAsync(async () =>
    {
        var count = await Sender.Send(new GenerateSalaryCertificatesCommand(Year));
        await LoadAsync();
        StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Payroll_CertificatesGenerated"], count);
    });

    [RelayCommand]
    private Task PrintAsync() => SelectedRow is null
        ? Task.CompletedTask
        : RunAsync(async () => _ = await _printService.PrintAsync(ReportDocumentType.SalaryCertificate, SelectedRow.Certificate.Id));
}
