// <copyright file="ReceivablesReconcileViewModel.cs" company="Fattura Facile">
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
/// The reconciliation dialog of one open item: partial or full settlement by payment (also from the unreconciled payments), credit
/// note, discount or write-off, or a debit adjustment, with a live preview of the open amount and status afterwards.
/// </summary>
internal sealed partial class ReceivablesReconcileViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;
    private readonly ReceivableItem _item;
    private readonly IReadOnlyList<UnreconciledPayment> _payments;
    private readonly Func<Task>? _completed;

    [ObservableProperty]
    private ReceivablesChoice? _selectedType;

    [ObservableProperty]
    private string _amountText = string.Empty;

    [ObservableProperty]
    private DateTime _valueDate;

    [ObservableProperty]
    private string _reference = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private ReceivablesChoice? _selectedPayment;

    [ObservableProperty]
    private string _creditNoteId = string.Empty;

    [ObservableProperty]
    private bool _showPayments;

    [ObservableProperty]
    private bool _showCreditNote;

    [ObservableProperty]
    private string _previewText = string.Empty;

    [ObservableProperty]
    private string _resultText = string.Empty;

    [ObservableProperty]
    private string _resultState = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesReconcileViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    /// <param name="item">The item to settle.</param>
    /// <param name="payments">The unreconciled payments of the customer.</param>
    /// <param name="completed">Called after the allocation was saved or queued, to refresh the views.</param>
    public ReceivablesReconcileViewModel(ISender sender, ILocalizationService localization, TimeProvider time, ReceivableItem item, IReadOnlyList<UnreconciledPayment> payments, Func<Task>? completed)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(payments);

        _sender = sender;
        _localization = localization;
        _time = time;
        _item = item;
        _payments = payments;
        _completed = completed;

        foreach (var type in Enum.GetValues<AllocationType>())
        {
            TypeChoices.Add(new ReceivablesChoice(type.ToWire(), _localization["Receivables_Type_" + type.ToWire()]));
        }

        PaymentChoices.Add(new ReceivablesChoice(string.Empty, _localization["Receivables_NoPayment"]));
        foreach (var payment in payments.Where(candidate => candidate.Unallocated > 0m))
        {
            var culture = _localization.CurrentCulture;
            PaymentChoices.Add(new ReceivablesChoice(
                payment.LedgerEntryId.ToString("D", CultureInfo.InvariantCulture),
                string.Format(culture, _localization["Receivables_PaymentChoice"], payment.Reference, ReceivablesFormat.Day(payment.Date, culture), ReceivablesFormat.Money(payment.Unallocated, culture))));
        }

        _selectedPayment = PaymentChoices[0];
        _valueDate = _time.GetLocalNow().Date;
        _selectedType = TypeChoices[0];
        UpdateVisibility();
    }

    /// <summary>
    /// Gets the allocation kinds.
    /// </summary>
    public ObservableCollection<ReceivablesChoice> TypeChoices { get; } = [];

    /// <summary>
    /// Gets the unreconciled payments the amount can be taken from, after the entry that means none.
    /// </summary>
    public ObservableCollection<ReceivablesChoice> PaymentChoices { get; } = [];

    /// <summary>
    /// Gets the number of the document being settled.
    /// </summary>
    public string DocumentNumber => _item.DocumentNumber;

    /// <summary>
    /// Gets the open amount of the item.
    /// </summary>
    public string OpenText => ReceivablesFormat.Money(_item.OpenAmount, _localization.CurrentCulture);

    /// <summary>
    /// Saves the allocation; it is sent now when the server is reachable and queued otherwise.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SubmitAsync()
    {
        var culture = _localization.CurrentCulture;
        if (!ReceivablesFormat.TryParseAmount(AmountText, culture, out var amount))
        {
            Show(_localization[ReconciliationRules.AmountKey], ReceivablesFormat.Bad);

            return;
        }

        var type = SelectedAllocationType();
        var payment = SelectedPaymentModel();
        var problem = ReconciliationRules.Validate(_item, type, amount, payment);
        if (problem is not null)
        {
            Show(_localization[problem], ReceivablesFormat.Bad);

            return;
        }

        var source = type switch
        {
            AllocationType.Payment => payment?.LedgerEntryId.ToString("D", CultureInfo.InvariantCulture),
            AllocationType.CreditNote => CreditNoteId.Trim(),
            _ => null,
        };

        IsBusy = true;

        try
        {
            AllocationLine line = new(_item.Id, type, amount, DateOnly.FromDateTime(ValueDate), source, Reference, Notes);
            ApplyAllocationsRequest request = new(Guid.NewGuid(), _item.CustomerId, _item.DocumentNumber, [line], _time.GetUtcNow().UtcDateTime);
            var result = await _sender.Send(new ApplyReceivableAllocationsCommand(request));
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

    partial void OnSelectedTypeChanged(ReceivablesChoice? value)
    {
        UpdateVisibility();
        UpdatePreview();
    }

    partial void OnAmountTextChanged(string value) => UpdatePreview();

    partial void OnSelectedPaymentChanged(ReceivablesChoice? value)
    {
        if (SelectedPaymentModel() is { } payment && string.IsNullOrWhiteSpace(AmountText))
        {
            AmountText = ReceivablesFormat.Money(Math.Min(_item.OpenAmount, payment.Unallocated), _localization.CurrentCulture);
        }
    }

    [RelayCommand]
    private void SetFull()
    {
        AmountText = ReceivablesFormat.Money(_item.OpenAmount, _localization.CurrentCulture);
    }

    private void Show(string text, string state)
    {
        ResultText = text;
        ResultState = state;
    }

    private void UpdateVisibility()
    {
        var type = SelectedAllocationType();
        ShowPayments = type == AllocationType.Payment && PaymentChoices.Count > 1;
        ShowCreditNote = type == AllocationType.CreditNote;
    }

    private void UpdatePreview()
    {
        var culture = _localization.CurrentCulture;
        if (!ReceivablesFormat.TryParseAmount(AmountText, culture, out var amount))
        {
            PreviewText = string.Empty;

            return;
        }

        var type = SelectedAllocationType();
        var problem = ReconciliationRules.Validate(_item, type, amount, SelectedPaymentModel());
        if (problem is not null)
        {
            PreviewText = _localization[problem];

            return;
        }

        var preview = ReconciliationRules.Preview(_item, type, amount);
        PreviewText = string.Format(culture, _localization["Receivables_PreviewOpen"], ReceivablesFormat.Money(preview.OpenAfter, culture), _localization["Receivables_Status_" + preview.StatusAfter.ToWire()]);
    }

    private AllocationType SelectedAllocationType()
    {
        var key = SelectedType?.Key ?? string.Empty;

        foreach (var type in Enum.GetValues<AllocationType>())
        {
            if (string.Equals(type.ToWire(), key, StringComparison.Ordinal))
            {
                return type;
            }
        }

        return AllocationType.Payment;
    }

    private UnreconciledPayment? SelectedPaymentModel()
    {
        var key = SelectedPayment?.Key ?? string.Empty;

        return key.Length == 0
            ? null
            : _payments.FirstOrDefault(payment => string.Equals(payment.LedgerEntryId.ToString("D", CultureInfo.InvariantCulture), key, StringComparison.Ordinal));
    }
}
