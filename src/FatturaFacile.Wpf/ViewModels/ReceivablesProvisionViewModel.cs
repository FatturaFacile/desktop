// <copyright file="ReceivablesProvisionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The bad-debt provision per aging bucket with editable rates (live preview, reset to the defaults, save) and the DSO.
/// </summary>
internal sealed partial class ReceivablesProvisionViewModel : ObservableObject
{
    private const string RateFormat = "0.##";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;
    private readonly Func<Task>? _changed;

    private string? _currency;
    private bool _suppress;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _noticeText = string.Empty;

    [ObservableProperty]
    private string _totalOpenText = string.Empty;

    [ObservableProperty]
    private string _totalProvisionText = string.Empty;

    [ObservableProperty]
    private string _netRealizableText = string.Empty;

    [ObservableProperty]
    private string _periodDaysText = DsoCalculator.DefaultPeriodDays.ToString(CultureInfo.InvariantCulture);

    [ObservableProperty]
    private string _dsoText = string.Empty;

    [ObservableProperty]
    private string _dsoDetail = string.Empty;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _resultState = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesProvisionViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    /// <param name="changed">Called after the rates were saved, to refresh the dashboard.</param>
    public ReceivablesProvisionViewModel(ISender sender, ILocalizationService localization, TimeProvider time, Func<Task>? changed)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        _changed = changed;
    }

    /// <summary>
    /// Gets the provision per bucket.
    /// </summary>
    public ObservableCollection<ReceivablesProvisionRow> Rows { get; } = [];

    /// <summary>
    /// Loads the provision and the DSO.
    /// </summary>
    /// <param name="currency">The ISO currency, null for the company currency.</param>
    /// <returns>A task representing the load.</returns>
    public async Task LoadAsync(string? currency)
    {
        _currency = currency;
        IsBusy = true;

        try
        {
            var provision = await _sender.Send(new GetBadDebtProvisionQuery(null, currency));
            ApplyProvision(provision);
            var days = int.TryParse(PeriodDaysText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : DsoCalculator.DefaultPeriodDays;
            var dso = await _sender.Send(new GetDsoQuery(null, days, currency));
            ApplyDso(dso);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync(_currency);

    [RelayCommand]
    private void ResetDefaults()
    {
        _suppress = true;
        try
        {
            for (var index = 0; index < Rows.Count; index++)
            {
                Rows[index].RateText = ProvisionRates.DefaultFor(index).ToString(RateFormat, _localization.CurrentCulture);
                Rows[index].IsDefault = true;
            }
        }
        finally
        {
            _suppress = false;
        }

        Recalculate();
    }

    [RelayCommand]
    private async Task SaveRatesAsync()
    {
        if (!TryReadRates(out var rates))
        {
            Show(_localization[ReceivablesService.RateKey], ReceivablesFormat.Bad);

            return;
        }

        IsBusy = true;

        try
        {
            var result = await _sender.Send(new UpdateProvisionRatesCommand(new ProvisionRatesRequest(Guid.NewGuid(), rates, _time.GetUtcNow().UtcDateTime)));
            var (text, state) = ReceivablesFormat.Describe(result, _localization);
            Show(text, state);

            if (result.IsAccepted && _changed is not null)
            {
                await _changed();
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Show(ex.Message, ReceivablesFormat.Bad);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Show(string text, string state)
    {
        ResultText = text;
        ResultState = state;
    }

    private void ApplyProvision(ReceivablesLoad<ProvisionSummary>? load)
    {
        Rows.Clear();

        if (load is null)
        {
            StatusMessage = _localization["Receivables_NoData"];
            NoticeText = string.Empty;
            TotalOpenText = string.Empty;
            TotalProvisionText = string.Empty;
            NetRealizableText = string.Empty;

            return;
        }

        var culture = _localization.CurrentCulture;
        NoticeText = load.FromCache ? _localization["Receivables_OfflineCache"] : string.Empty;
        StatusMessage = string.Empty;

        _suppress = true;
        try
        {
            foreach (var line in load.Data.Lines)
            {
                AgingBucket bucket = new(line.Key, line.From, line.To);
                Rows.Add(new ReceivablesProvisionRow(
                    bucket,
                    ReceivablesFormat.BucketLabel(bucket, _localization),
                    line.OpenAmount,
                    ReceivablesFormat.Money(line.OpenAmount, culture),
                    line.RatePercent.ToString(RateFormat, culture),
                    ReceivablesFormat.Money(line.Provision, culture),
                    line.IsDefaultRate,
                    OnRateEdited));
            }
        }
        finally
        {
            _suppress = false;
        }

        TotalOpenText = ReceivablesFormat.Money(load.Data.TotalOpen, culture);
        TotalProvisionText = ReceivablesFormat.Money(load.Data.TotalProvision, culture);
        NetRealizableText = ReceivablesFormat.Money(load.Data.NetRealizableValue, culture);
    }

    private void ApplyDso(ReceivablesLoad<DsoResult>? load)
    {
        if (load is null)
        {
            DsoText = string.Empty;
            DsoDetail = string.Empty;

            return;
        }

        var culture = _localization.CurrentCulture;
        var dso = load.Data;
        DsoText = dso.Dso is { } days
            ? string.Format(culture, _localization["Receivables_DsoDays"], days.ToString("0.##", culture))
            : _localization["Receivables_DsoUndefined"];
        DsoDetail = string.Format(culture, _localization["Receivables_DsoDetail"], ReceivablesFormat.Money(dso.Receivables, culture), ReceivablesFormat.Money(dso.CreditSales, culture), dso.PeriodDays.ToString(culture));
    }

    private void OnRateEdited(ReceivablesProvisionRow row)
    {
        if (!_suppress)
        {
            row.IsDefault = false;
            Recalculate();
        }
    }

    private bool TryReadRates(out Dictionary<string, decimal> rates)
    {
        rates = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var culture = _localization.CurrentCulture;

        foreach (var row in Rows)
        {
            if (!ReceivablesFormat.TryParseAmount(row.RateText, culture, out var rate) || !ProvisionRates.IsValid(rate))
            {
                return false;
            }

            rates[row.Bucket.Key] = rate;
        }

        return rates.Count > 0;
    }

    private void Recalculate()
    {
        if (!TryReadRates(out var rates))
        {
            StatusMessage = _localization[ReceivablesService.RateKey];

            return;
        }

        StatusMessage = string.Empty;
        var culture = _localization.CurrentCulture;
        var totals = Rows.ToDictionary(row => row.Bucket.Key, row => row.OpenAmount, StringComparer.Ordinal);
        var summary = BadDebtProvisionCalculator.Calculate([.. Rows.Select(row => row.Bucket)], totals, rates);

        foreach (var line in summary.Lines)
        {
            var row = Rows.First(candidate => string.Equals(candidate.Bucket.Key, line.Key, StringComparison.Ordinal));
            row.ProvisionText = ReceivablesFormat.Money(line.Provision, culture);
        }

        TotalProvisionText = ReceivablesFormat.Money(summary.TotalProvision, culture);
        NetRealizableText = ReceivablesFormat.Money(summary.NetRealizableValue, culture);
    }
}
