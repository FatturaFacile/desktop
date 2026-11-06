// <copyright file="PayrollInputsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Application.Payroll.Swiss;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Monthly inputs (hours, overtime, bonuses, expenses, benefits in kind, daily allowances, deductions).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PayrollInputsViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed partial class PayrollInputsViewModel(ISender sender, ILocalizationService localizationService) : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private int _year = DateTime.Today.Year;

    [ObservableProperty]
    private int _month = DateTime.Today.Month;

    [ObservableProperty]
    private PayrollInput _newInput = new();

    [ObservableProperty]
    private PayrollInputRow? _selectedRow;

    /// <summary>
    /// Gets the inputs of the month.
    /// </summary>
    public ObservableCollection<PayrollInputRow> Rows { get; } = [];

    /// <summary>
    /// Gets the employees.
    /// </summary>
    public ObservableCollection<HrEmployee> Employees { get; } = [];

    /// <summary>
    /// Gets the wage types that can be entered manually.
    /// </summary>
    public ObservableCollection<PayrollWageType> WageTypes { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var year = Year;
        var month = Month;
        var employees = await Sender.Send(new GetPayrollRecordsQuery<HrEmployee>());
        var wageTypes = await Sender.Send(new GetPayrollRecordsQuery<PayrollWageType>(w => w.IsActive));
        var inputs = await Sender.Send(new GetPayrollRecordsQuery<PayrollInput>(i => i.Year == year && i.Month == month));

        Replace(Employees, employees.OrderBy(static e => e.LastName, StringComparer.CurrentCulture));
        Replace(WageTypes, wageTypes.Where(static w => !string.Equals(w.Kind, WageTypeKind.Employer, StringComparison.Ordinal)).OrderBy(static w => w.Code, StringComparer.Ordinal));

        var names = employees.ToDictionary(static e => e.Id, static e => e.FullName);
        var wageNames = wageTypes.ToDictionary(static w => w.Code, static w => w.Name, StringComparer.Ordinal);
        Rows.Clear();

        foreach (var input in inputs.OrderBy(i => names.GetValueOrDefault(i.EmployeeId), StringComparer.CurrentCulture).ThenBy(static i => i.WageTypeCode, StringComparer.Ordinal))
        {
            Rows.Add(new PayrollInputRow(input, names.GetValueOrDefault(input.EmployeeId) ?? "?", wageNames.GetValueOrDefault(input.WageTypeCode) ?? input.WageTypeCode));
        }
    });

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();

        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    partial void OnYearChanged(int value) => _ = LoadAsync();

    partial void OnMonthChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    private Task AddAsync() => RunAsync(
        async () =>
        {
            NewInput.Year = Year;
            NewInput.Month = Month;
            _ = await Sender.Send(new SavePayrollRecordCommand<PayrollInput>(NewInput));
            NewInput = new PayrollInput { EmployeeId = NewInput.EmployeeId };
            await LoadAsync();
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteAsync() => SelectedRow is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            _ = await Sender.Send(new DeletePayrollRecordCommand<PayrollInput>(SelectedRow.Input.Id));
            await LoadAsync();
        });
}
