// <copyright file="PayrollEmployeesViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Employees with their Swiss payroll data, children (family allowances) and absences.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PayrollEmployeesViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed partial class PayrollEmployeesViewModel(ISender sender, ILocalizationService localizationService) : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private HrEmployee? _selectedEmployee;

    [ObservableProperty]
    private HrEmployee _editing = NewEmployee();

    [ObservableProperty]
    private HrEmployeeChild _newChild = new() { DateOfBirth = DateTime.Today.AddYears(-5) };

    [ObservableProperty]
    private HrEmployeeChild? _selectedChild;

    [ObservableProperty]
    private HrAbsence _newAbsence = new();

    [ObservableProperty]
    private HrAbsence? _selectedAbsence;

    /// <summary>
    /// Gets the employees.
    /// </summary>
    public ObservableCollection<HrEmployee> Employees { get; } = [];

    /// <summary>
    /// Gets the children of the selected employee.
    /// </summary>
    public ObservableCollection<HrEmployeeChild> Children { get; } = [];

    /// <summary>
    /// Gets the absences of the selected employee.
    /// </summary>
    public ObservableCollection<HrAbsence> Absences { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedEmployee?.Id;
        var employees = await Sender.Send(new GetPayrollRecordsQuery<HrEmployee>());
        Employees.Clear();

        foreach (var employee in employees.OrderBy(static e => e.LastName, StringComparer.CurrentCulture).ThenBy(static e => e.FirstName, StringComparer.CurrentCulture))
        {
            Employees.Add(employee);
        }

        SelectedEmployee = Employees.FirstOrDefault(e => e.Id == selectedId) ?? Employees.FirstOrDefault();
    });

    private static HrEmployee NewEmployee() => new()
    {
        Id = Guid.Empty,
        Currency = "CHF",
        SalaryInstallmentsPerYear = 13,
        ResidenceCountry = "CH",
        ResidencePermit = "CH",
        HireDate = DateTime.Today,
    };

    partial void OnSelectedEmployeeChanged(HrEmployee? value)
    {
        Editing = value is null ? NewEmployee() : RecordCopier.Clone(value);
        _ = LoadDetailsAsync();
    }

    [RelayCommand]
    private void New()
    {
        SelectedEmployee = null;
        Editing = NewEmployee();
        Children.Clear();
        Absences.Clear();
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(
        async () =>
        {
            var id = await Sender.Send(new SavePayrollRecordCommand<HrEmployee>(Editing));
            await LoadAsync();
            SelectedEmployee = Employees.FirstOrDefault(e => e.Id == id);
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteAsync() => SelectedEmployee is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeletePayrollRecordCommand<HrEmployee>(SelectedEmployee.Id));
                await LoadAsync();
            },
            "Common_Deleted");

    [RelayCommand]
    private Task AddChildAsync() => SelectedEmployee is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            NewChild.EmployeeId = SelectedEmployee.Id;
            _ = await Sender.Send(new SavePayrollRecordCommand<HrEmployeeChild>(NewChild));
            NewChild = new HrEmployeeChild { DateOfBirth = DateTime.Today.AddYears(-5), LastName = SelectedEmployee.LastName };
            await LoadDetailsAsync();
        });

    [RelayCommand]
    private Task RemoveChildAsync() => SelectedChild is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            _ = await Sender.Send(new DeletePayrollRecordCommand<HrEmployeeChild>(SelectedChild.Id));
            await LoadDetailsAsync();
        });

    [RelayCommand]
    private Task AddAbsenceAsync() => SelectedEmployee is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            NewAbsence.EmployeeId = SelectedEmployee.Id;
            _ = await Sender.Send(new SavePayrollRecordCommand<HrAbsence>(NewAbsence));
            NewAbsence = new HrAbsence();
            await LoadDetailsAsync();
        });

    [RelayCommand]
    private Task RemoveAbsenceAsync() => SelectedAbsence is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            _ = await Sender.Send(new DeletePayrollRecordCommand<HrAbsence>(SelectedAbsence.Id));
            await LoadDetailsAsync();
        });

    private async Task LoadDetailsAsync()
    {
        Children.Clear();
        Absences.Clear();

        if (SelectedEmployee is not { } employee)
        {
            return;
        }

        var employeeId = employee.Id;
        var children = await Sender.Send(new GetPayrollRecordsQuery<HrEmployeeChild>(c => c.EmployeeId == employeeId));
        var absences = await Sender.Send(new GetPayrollRecordsQuery<HrAbsence>(a => a.EmployeeId == employeeId));

        foreach (var child in children.OrderBy(static c => c.DateOfBirth))
        {
            Children.Add(child);
        }

        foreach (var absence in absences.OrderByDescending(static a => a.StartDate))
        {
            Absences.Add(absence);
        }
    }
}
