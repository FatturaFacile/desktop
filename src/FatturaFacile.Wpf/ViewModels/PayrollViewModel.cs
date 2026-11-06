// <copyright file="PayrollViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing HR personnel and interactive localized payroll deductions calculation (CH vs IT).
/// </summary>
internal sealed partial class PayrollViewModel : ObservableObject
{
    private readonly ISender _sender;

    private readonly ICurrentTenantService _currentTenantService;

    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<HrEmployeeDto> _employees = [];

    [ObservableProperty]
    private HrEmployeeDto? _selectedEmployee;

    [ObservableProperty]
    private PayrollCalculationResultDto? _currentCalculationResult;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    public PayrollViewModel(
        ISender               sender,
        ICurrentTenantService currentTenantService,
        ILocalizationService  localizationService)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(currentTenantService);
        ArgumentNullException.ThrowIfNull(localizationService);

        _sender = sender;
        _currentTenantService = currentTenantService;
        _localizationService = localizationService;
        _statusMessage = localizationService["Common_Ready"];

        _currentTenantService.CurrentTenantChanged += (_, _) => _ = OnTenantChangedAsync();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollViewModel"/> class with the Swiss payroll module.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="swiss">The Swiss payroll module shown for Swiss companies.</param>
    public PayrollViewModel(
        ISender sender,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService,
        SwissPayrollViewModel swiss)
        : this(sender, currentTenantService, localizationService)
    {
        Swiss = swiss;
    }

    /// <summary>
    /// Gets the Swiss payroll module (Swissdec payroll core), null in the simplified preview mode.
    /// </summary>
    public SwissPayrollViewModel? Swiss { get; }

    /// <summary>
    /// Gets a value indicating whether the Swiss payroll module is shown (Swiss company).
    /// </summary>
    public bool IsSwiss => Swiss is not null && string.Equals(_currentTenantService.CountryCode, "CH", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the standard payroll preview is shown.
    /// </summary>
    public bool IsStandard => !IsSwiss;

    /// <summary>
    /// Loads employee directory for current tenant.
    /// </summary>
    /// <returns>A task representing the load operation.</returns>
    [RelayCommand]
    public async Task LoadEmployeesAsync()
    {
        OnPropertyChanged(nameof(IsSwiss));
        OnPropertyChanged(nameof(IsStandard));

        if (IsSwiss && Swiss is not null)
        {
            await Swiss.LoadAsync();
        }

        IsLoading = true;
        StatusMessage = _localizationService["Common_Loading"];

        try
        {
            var employees = await _sender.Send(new GetEmployeesQuery());
            Employees.Clear();

            foreach (var emp in employees)
            {
                Employees.Add(emp);
            }

            SelectedEmployee = Employees.FirstOrDefault();
            StatusMessage = $"{Employees.Count} {_localizationService["Nav_Payroll"]}.";

            if (SelectedEmployee != null)
            {
                await CalculatePayrollAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Calculates localized payroll preview based on employee's country rules.
    /// </summary>
    /// <returns>A task representing payroll calculation.</returns>
    [RelayCommand]
    public async Task CalculatePayrollAsync()
    {
        if (SelectedEmployee == null)
        {
            StatusMessage = _localizationService["Common_Ready"];
            return;
        }

        IsLoading = true;

        try
        {
            CurrentCalculationResult = await _sender.Send(
                new CalculatePayrollPreviewQuery(SelectedEmployee.Id));

            if (CurrentCalculationResult != null)
            {
                StatusMessage = string.Create(
                    CultureInfo.CurrentCulture,
                    $"{_localizationService["Payroll_MonthlyNet"]}: {CurrentCalculationResult.NetMonthlySalary:C2} ({CurrentCalculationResult.Currency})");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task OnTenantChangedAsync()
    {
        SelectedEmployee = null;
        CurrentCalculationResult = null;
        Employees.Clear();
        return LoadEmployeesAsync();
    }
}
