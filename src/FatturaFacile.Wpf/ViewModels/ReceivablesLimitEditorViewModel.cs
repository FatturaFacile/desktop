// <copyright file="ReceivablesLimitEditorViewModel.cs" company="Fattura Facile">
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
/// The dialog that creates or edits a credit limit: amount, insurance, overdue tolerance, reaction policy and validity.
/// </summary>
internal sealed partial class ReceivablesLimitEditorViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;
    private readonly Guid _customerId;
    private readonly string _customerLabel;
    private readonly Guid? _limitId;
    private readonly Func<Task>? _completed;

    [ObservableProperty]
    private string _currency;

    [ObservableProperty]
    private string _grantedText = string.Empty;

    [ObservableProperty]
    private string _insuredText = string.Empty;

    [ObservableProperty]
    private string _insurerName = string.Empty;

    [ObservableProperty]
    private string _policyNumber = string.Empty;

    [ObservableProperty]
    private string _coverageText = string.Empty;

    [ObservableProperty]
    private string _maxOverdueDaysText = string.Empty;

    [ObservableProperty]
    private string _toleranceText = string.Empty;

    [ObservableProperty]
    private ReceivablesChoice? _selectedPolicy;

    [ObservableProperty]
    private DateTime _validFrom;

    [ObservableProperty]
    private DateTime? _validTo;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _resultState = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesLimitEditorViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    /// <param name="customerId">The customer.</param>
    /// <param name="customerLabel">The customer name for the queue list.</param>
    /// <param name="existing">The limit to edit, null to create one.</param>
    /// <param name="defaultCurrency">The currency of a new limit.</param>
    /// <param name="completed">Called after the limit was saved or queued, to refresh the views.</param>
    public ReceivablesLimitEditorViewModel(ISender sender, ILocalizationService localization, TimeProvider time, Guid customerId, string customerLabel, CustomerCreditLimit? existing, string defaultCurrency, Func<Task>? completed)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        _customerId = customerId;
        _customerLabel = customerLabel;
        _limitId = existing?.Id;
        _completed = completed;
        _currency = defaultCurrency;
        _validFrom = _time.GetLocalNow().Date;

        foreach (var policy in Enum.GetValues<CreditReactionPolicy>())
        {
            PolicyChoices.Add(new ReceivablesChoice(policy.ToWire(), _localization["Receivables_Policy_" + policy.ToWire()]));
        }

        _selectedPolicy = PolicyChoices[0];

        if (existing is not null)
        {
            var culture = _localization.CurrentCulture;
            _currency = existing.Currency;
            _grantedText = existing.GrantedLimit.ToString("0.00", culture);
            _insuredText = existing.InsuredLimit > 0m ? existing.InsuredLimit.ToString("0.00", culture) : string.Empty;
            _insurerName = existing.InsurerName ?? string.Empty;
            _policyNumber = existing.PolicyNumber ?? string.Empty;
            _coverageText = existing.CoveragePercentage > 0m ? existing.CoveragePercentage.ToString("0.##", culture) : string.Empty;
            _maxOverdueDaysText = existing.MaxOverdueDays?.ToString(culture) ?? string.Empty;
            _toleranceText = existing.OverdueToleranceAmount > 0m ? existing.OverdueToleranceAmount.ToString("0.00", culture) : string.Empty;
            _selectedPolicy = PolicyChoices.First(choice => string.Equals(choice.Key, existing.ReactionPolicy.ToWire(), StringComparison.Ordinal));
            _validFrom = existing.ValidFrom.ToDateTime(TimeOnly.MinValue);
            _validTo = existing.ValidTo?.ToDateTime(TimeOnly.MinValue);
            _isActive = existing.IsActive;
            _notes = existing.Notes ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the reaction policies.
    /// </summary>
    public ObservableCollection<ReceivablesChoice> PolicyChoices { get; } = [];

    /// <summary>
    /// Gets a value indicating whether an existing limit is edited.
    /// </summary>
    public bool IsEdit => _limitId is not null;

    /// <summary>
    /// Saves the limit; it is sent now when the server is reachable and queued otherwise.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SubmitAsync()
    {
        var culture = _localization.CurrentCulture;
        if (!ReceivablesFormat.TryParseAmount(GrantedText, culture, out var granted)
            || !TryOptional(InsuredText, culture, out var insured)
            || !TryOptional(CoverageText, culture, out var coverage)
            || !TryOptional(ToleranceText, culture, out var tolerance)
            || !TryOptionalDays(MaxOverdueDaysText, culture, out var days))
        {
            Show(_localization[ReceivablesService.LimitKey], ReceivablesFormat.Bad);

            return;
        }

        IsBusy = true;

        try
        {
            CreditLimitRequest request = new(
                Guid.NewGuid(),
                _customerId,
                _customerLabel,
                _limitId,
                Currency,
                granted,
                insured,
                InsurerName,
                PolicyNumber,
                coverage,
                days,
                tolerance,
                ReceivablesWire.ParsePolicy(SelectedPolicy?.Key),
                DateOnly.FromDateTime(ValidFrom),
                ValidTo is { } to ? DateOnly.FromDateTime(to) : null,
                IsActive,
                Notes,
                _time.GetUtcNow().UtcDateTime);
            var result = await _sender.Send(new SaveCreditLimitCommand(request));
            var (text, state) = ReceivablesFormat.Describe(result, _localization);
            Show(text, state);

            if (result.IsAccepted && _completed is not null)
            {
                await _completed();
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

    private static bool TryOptional(string? text, CultureInfo culture, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!ReceivablesFormat.TryParseAmount(text, culture, out var parsed))
        {
            return false;
        }

        value = parsed;

        return true;
    }

    private static bool TryOptionalDays(string? text, CultureInfo culture, out int? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!int.TryParse(text, NumberStyles.Integer, culture, out var parsed))
        {
            return false;
        }

        value = parsed;

        return true;
    }

    private void Show(string text, string state)
    {
        ResultText = text;
        ResultState = state;
    }
}
