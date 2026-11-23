// <copyright file="LocalHolidaysViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the holidays of the capacity planning: the local holidays of the tenant are listed, added and removed (after a confirmation),
/// and the holidays the planning resolves for a place and a year (national, cantonal or regional, municipal and local) are shown.
/// </summary>
internal sealed partial class LocalHolidaysViewModel : ObservableObject
{
    private const string DefaultFraction = "1";
    private const int FirstYear = 1900;
    private const int LastYear = 2200;
    private const int LeapYear = 2024;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;
    private bool _resolvedShown;

    [ObservableProperty]
    private PlanningChoice? _selectedCountry;

    [ObservableProperty]
    private string _regionText = string.Empty;

    [ObservableProperty]
    private string _municipalityText = string.Empty;

    [ObservableProperty]
    private string _monthText = string.Empty;

    [ObservableProperty]
    private string _dayText = string.Empty;

    [ObservableProperty]
    private string _yearText = string.Empty;

    [ObservableProperty]
    private string _nameText = string.Empty;

    [ObservableProperty]
    private string _fractionText = DefaultFraction;

    [ObservableProperty]
    private LocalHolidayRow? _selectedRow;

    [ObservableProperty]
    private bool _isDeletePending;

    [ObservableProperty]
    private PlanningChoice? _resolveCountry;

    [ObservableProperty]
    private string _resolveRegionText = string.Empty;

    [ObservableProperty]
    private string _resolveMunicipalityText = string.Empty;

    [ObservableProperty]
    private string _resolveYearText;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private bool _hasResolved;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalHolidaysViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public LocalHolidaysViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);

        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
        CountryChoices = [.. PlanningDraftBuilder.Countries.Select(code => new PlanningChoice(code, _localization["ResourcePlanning_Country_" + code]))];
        _selectedCountry = CountryChoices[0];
        _resolveCountry = CountryChoices[0];
        _resolveYearText = time.GetLocalNow().Year.ToString(_format.Culture);
    }

    /// <summary>
    /// Gets the countries a holiday can be given for.
    /// </summary>
    public IReadOnlyList<PlanningChoice> CountryChoices { get; }

    /// <summary>
    /// Gets the local holidays of the tenant.
    /// </summary>
    public ObservableCollection<LocalHolidayRow> Rows { get; } = [];

    /// <summary>
    /// Gets the holidays resolved for the place and the year.
    /// </summary>
    public ObservableCollection<HolidayRow> ResolvedRows { get; } = [];

    /// <summary>
    /// Loads the local holidays of the tenant.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        var failure = await PlanningCalls.ReadAsync(ReloadAsync, _localization);
        StatusMessage = failure ?? StatusMessage;
        IsBusy = false;
    }

    /// <summary>
    /// Adds the local holiday of the form.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task AddAsync()
    {
        var yearText = YearText.Trim();
        var year = _format.ParseInt(yearText);
        if (yearText.Length > 0 && year is null)
        {
            StatusMessage = _format.Describe("Setup", PlanningDraftBuilder.HolidayInvalidDate);

            return;
        }

        var built = PlanningDraftBuilder.LocalHoliday(SelectedCountry?.Key, RegionText, MunicipalityText, _format.ParseInt(MonthText), _format.ParseInt(DayText), year, NameText, _format.ParseDecimal(FractionText));
        if (built.Request is not { } request)
        {
            StatusMessage = _format.Describe("Setup", built.ErrorCode ?? string.Empty);

            return;
        }

        IsBusy = true;
        var result = await PlanningCalls.WriteAsync(() => _sender.Send(new CreateLocalHolidayCommand(request)), _format, _localization);
        if (result.IsSuccess)
        {
            StatusMessage = _localization["ResourcePlanning_Holidays_Added"];
            ClearForm();
            await ReloadAfterWriteAsync();
        }
        else
        {
            StatusMessage = result.Message;
        }

        IsBusy = false;
    }

    /// <summary>
    /// Asks the user to confirm the removal of the selected local holiday; nothing is removed yet.
    /// </summary>
    [RelayCommand]
    public void RequestDelete()
    {
        if (SelectedRow is null)
        {
            StatusMessage = _localization["ResourcePlanning_Holidays_NeedsSelection"];

            return;
        }

        IsDeletePending = true;
        StatusMessage = string.Format(_format.Culture, _localization["ResourcePlanning_Holidays_DeleteConfirm"], SelectedRow.Holiday.Name, SelectedRow.DateText);
    }

    /// <summary>
    /// Withdraws the request to remove the selected local holiday.
    /// </summary>
    [RelayCommand]
    public void CancelDelete()
    {
        IsDeletePending = false;
        StatusMessage = string.Empty;
    }

    /// <summary>
    /// Removes the selected local holiday once the user confirmed it.
    /// </summary>
    /// <returns>A task representing the removal.</returns>
    [RelayCommand]
    public async Task ConfirmDeleteAsync()
    {
        if (!IsDeletePending || SelectedRow is not { } row)
        {
            return;
        }

        IsDeletePending = false;
        IsBusy = true;
        var result = await PlanningCalls.WriteAsync(() => _sender.Send(new DeleteLocalHolidayCommand(row.Holiday.Id)), _format, _localization);
        if (result.IsSuccess)
        {
            StatusMessage = _localization["ResourcePlanning_Holidays_Removed"];
            await ReloadAfterWriteAsync();
        }
        else
        {
            StatusMessage = result.Message;
        }

        IsBusy = false;
    }

    /// <summary>
    /// Shows the holidays the planning resolves for the place and the year of the form.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task ShowResolvedAsync()
    {
        if (!TryResolveRequest(out var request))
        {
            StatusMessage = _format.Describe("Setup", PlanningDraftBuilder.HolidayInvalidDate);

            return;
        }

        IsBusy = true;
        _resolvedShown = true;
        var failure = await PlanningCalls.ReadAsync(() => LoadResolvedAsync(request), _localization);
        StatusMessage = failure ?? StatusMessage;
        IsBusy = false;
    }

    private static string ScopeOf(LocalHoliday holiday)
    {
        var parts = new List<string> { holiday.Country };
        if (!string.IsNullOrWhiteSpace(holiday.RegionCode))
        {
            parts.Add(holiday.RegionCode);
        }

        if (!string.IsNullOrWhiteSpace(holiday.Municipality))
        {
            parts.Add(holiday.Municipality);
        }

        return string.Join(" / ", parts);
    }

    partial void OnSelectedRowChanged(LocalHolidayRow? value) => IsDeletePending = false;

    private bool TryResolveRequest(out GetPlanningHolidaysQuery request)
    {
        var year = _format.ParseInt(ResolveYearText);
        var country = ResolveCountry?.Key;
        if (country is null || year is not { } value || value < FirstYear || value > LastYear)
        {
            request = new GetPlanningHolidaysQuery(string.Empty, Region: null, Municipality: null, 0);

            return false;
        }

        request = new GetPlanningHolidaysQuery(
            country,
            string.IsNullOrWhiteSpace(ResolveRegionText) ? null : ResolveRegionText.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(ResolveMunicipalityText) ? null : ResolveMunicipalityText.Trim(),
            value);

        return true;
    }

    private void ClearForm()
    {
        NameText = string.Empty;
        MonthText = string.Empty;
        DayText = string.Empty;
        YearText = string.Empty;
        FractionText = DefaultFraction;
    }

    private async Task ReloadAfterWriteAsync()
    {
        var failure = await PlanningCalls.ReadAsync(ReloadAsync, _localization);
        StatusMessage = failure ?? StatusMessage;
    }

    private async Task ReloadAsync()
    {
        var holidays = await _sender.Send(new GetLocalHolidaysQuery());

        Rows.Clear();
        foreach (var holiday in holidays.OrderBy(item => item.Month).ThenBy(item => item.Day).ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            Rows.Add(new LocalHolidayRow(holiday, ScopeOf(holiday), DateOf(holiday), FractionOf(holiday.DayFraction)));
        }

        HasData = Rows.Count > 0;
        SelectedRow = null;
        if (!HasData)
        {
            StatusMessage = _localization["ResourcePlanning_Holidays_Empty"];
        }

        if (_resolvedShown && TryResolveRequest(out var request))
        {
            await LoadResolvedAsync(request);
        }
    }

    private async Task LoadResolvedAsync(GetPlanningHolidaysQuery request)
    {
        var holidays = await _sender.Send(request);

        ResolvedRows.Clear();
        foreach (var holiday in holidays.OrderBy(item => item.Date))
        {
            ResolvedRows.Add(new HolidayRow(_format.Day(holiday.Date), holiday.Name, FractionOf(holiday.DayFraction)));
        }

        HasResolved = ResolvedRows.Count > 0;
        if (!HasResolved)
        {
            StatusMessage = _localization["ResourcePlanning_Holidays_ResolvedEmpty"];
        }
    }

    private string FractionOf(decimal fraction) => fraction.ToString("0.##", _format.Culture);

    private string DateOf(LocalHoliday holiday)
    {
        var yearly = new DateOnly(holiday.Year ?? LeapYear, holiday.Month, holiday.Day);

        return holiday.Year is null
            ? yearly.ToString("m", _format.Culture)
            : yearly.ToString("d", _format.Culture);
    }
}
