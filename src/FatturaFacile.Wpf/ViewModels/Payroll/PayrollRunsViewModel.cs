// <copyright file="PayrollRunsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Monthly payroll runs: calculation, payslips with their lines, closing, printing and entry/exit notifications.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PayrollRunsViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="printService">The print service.</param>
internal sealed partial class PayrollRunsViewModel(ISender sender, ILocalizationService localizationService, IReportPrintService printService) : PayrollSectionViewModel(sender, localizationService)
{
    private readonly IReportPrintService _printService = printService;

    [ObservableProperty]
    private int _year = DateTime.Today.Year;

    [ObservableProperty]
    private int _month = DateTime.Today.Month;

    [ObservableProperty]
    private DateTime? _paymentDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 25, 0, 0, 0, DateTimeKind.Unspecified);

    [ObservableProperty]
    private PayrollRun? _selectedRun;

    [ObservableProperty]
    private Payslip? _selectedPayslip;

    /// <summary>
    /// Gets the payroll runs.
    /// </summary>
    public ObservableCollection<PayrollRun> Runs { get; } = [];

    /// <summary>
    /// Gets the payslips of the selected run.
    /// </summary>
    public ObservableCollection<Payslip> Payslips { get; } = [];

    /// <summary>
    /// Gets the lines of the selected payslip.
    /// </summary>
    public ObservableCollection<PayslipLine> Lines { get; } = [];

    /// <summary>
    /// Gets the entries and exits of the selected period.
    /// </summary>
    public ObservableCollection<EmploymentChangeDto> EmploymentChanges { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedRun?.Id;
        var runs = await Sender.Send(new GetPayrollRecordsQuery<PayrollRun>());
        Runs.Clear();

        foreach (var run in runs.OrderByDescending(static r => r.Year).ThenByDescending(static r => r.Month))
        {
            Runs.Add(run);
        }

        SelectedRun = Runs.FirstOrDefault(r => r.Id == selectedId) ?? Runs.FirstOrDefault();
        await LoadEmploymentChangesAsync();
    });

    partial void OnSelectedRunChanged(PayrollRun? value) => _ = LoadPayslipsAsync();

    partial void OnSelectedPayslipChanged(Payslip? value) => _ = LoadLinesAsync();

    partial void OnYearChanged(int value) => _ = LoadEmploymentChangesAsync();

    partial void OnMonthChanged(int value) => _ = LoadEmploymentChangesAsync();

    [RelayCommand]
    private Task CalculateAsync() => RunAsync(
        async () =>
        {
            var runId = await Sender.Send(new CalculatePayrollRunCommand(Year, Month, PaymentDate));
            await LoadAsync();
            SelectedRun = Runs.FirstOrDefault(r => r.Id == runId);
        },
        "Payroll_Calculated");

    [RelayCommand]
    private Task FinalizeAsync() => SelectedRun is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new FinalizePayrollRunCommand(SelectedRun.Id));
                await LoadAsync();
            },
            "Payroll_Finalized");

    [RelayCommand]
    private Task ReopenAsync() => SelectedRun is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new ReopenPayrollRunCommand(SelectedRun.Id));
                await LoadAsync();
            },
            "Payroll_Reopened");

    [RelayCommand]
    private Task PrintPayslipAsync() => SelectedPayslip is null
        ? Task.CompletedTask
        : RunAsync(async () => _ = await _printService.PrintAsync(ReportDocumentType.Payslip, SelectedPayslip.Id));

    private async Task LoadPayslipsAsync()
    {
        Payslips.Clear();
        Lines.Clear();

        if (SelectedRun is not { } run)
        {
            return;
        }

        var runId = run.Id;
        var payslips = await Sender.Send(new GetPayrollRecordsQuery<Payslip>(p => p.PayrollRunId == runId));

        foreach (var payslip in payslips.OrderBy(static p => p.EmployeeName, StringComparer.CurrentCulture))
        {
            Payslips.Add(payslip);
        }

        SelectedPayslip = Payslips.FirstOrDefault();
    }

    private async Task LoadLinesAsync()
    {
        Lines.Clear();

        if (SelectedPayslip is not { } payslip)
        {
            return;
        }

        var payslipId = payslip.Id;
        var lines = await Sender.Send(new GetPayrollRecordsQuery<PayslipLine>(l => l.PayslipId == payslipId));

        foreach (var line in lines.OrderBy(static l => l.SortOrder).ThenBy(static l => l.WageTypeCode, StringComparer.Ordinal))
        {
            Lines.Add(line);
        }
    }

    private async Task LoadEmploymentChangesAsync()
    {
        try
        {
            var changes = await Sender.Send(new GetEmploymentChangesQuery(Year, Month));
            EmploymentChanges.Clear();

            foreach (var change in changes)
            {
                EmploymentChanges.Add(change);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
