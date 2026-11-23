// <copyright file="PurchaseFulfilmentViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Procurement;
using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model of the purchase order fulfilment workbench (Mod 046): overdue orders with reminders, orders with supplier confirmation
/// and dispatch, dropshipments with transport document and delivery, the pricing preview and the offline queue. Every write goes
/// through the offline-first outbox, so the user can keep working without a connection.
/// </summary>
internal sealed partial class PurchaseFulfilmentViewModel : ObservableObject
{
    private const string DateTimeFormat = "g";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;

    private IReadOnlyList<PurchaseOrderView> _orderViews = [];
    private IReadOnlyList<OverdueOrderView> _overdueViews = [];
    private bool _suppressSelection;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _queueText = string.Empty;

    [ObservableProperty]
    private FulfilmentOverdueRow? _selectedOverdue;

    [ObservableProperty]
    private FulfilmentChoice? _selectedStatusFilter;

    [ObservableProperty]
    private FulfilmentOrderRow? _selectedOrder;

    [ObservableProperty]
    private bool _hasSelectedOrder;

    [ObservableProperty]
    private string _selectedOrderTitle = string.Empty;

    [ObservableProperty]
    private string _selectedOrderSummary = string.Empty;

    [ObservableProperty]
    private string _selectedChannel = ProcurementCodes.ChannelPdf;

    [ObservableProperty]
    private string _dispatchRecipient = string.Empty;

    [ObservableProperty]
    private string _confirmationNumber = string.Empty;

    [ObservableProperty]
    private DateTime? _confirmationDate;

    [ObservableProperty]
    private DateTime? _promisedDate;

    [ObservableProperty]
    private string _confirmationNotes = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="PurchaseFulfilmentViewModel"/> class.
    /// </summary>
    /// <param name="sender">The mediator.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public PurchaseFulfilmentViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        LastSyncedText = _localization["Procurement_NeverSynced"];
        QueueText = string.Empty;
        StatusFilters.Add(new FulfilmentChoice(Code: null, Label: _localization["Procurement_AllStatuses"]));
        foreach (var code in ProcurementCodes.OrderStatuses)
        {
            StatusFilters.Add(new FulfilmentChoice(code, _localization["Procurement_Status_" + code]));
        }

        _suppressSelection = true;
        SelectedStatusFilter = StatusFilters[0];
        _suppressSelection = false;
    }

    /// <summary>
    /// Gets the overdue orders.
    /// </summary>
    public ObservableCollection<FulfilmentOverdueRow> OverdueOrders { get; } = [];

    /// <summary>
    /// Gets the status filters of the order list.
    /// </summary>
    public ObservableCollection<FulfilmentChoice> StatusFilters { get; } = [];

    /// <summary>
    /// Gets the orders.
    /// </summary>
    public ObservableCollection<FulfilmentOrderRow> Orders { get; } = [];

    /// <summary>
    /// Gets the lines of the selected order.
    /// </summary>
    public ObservableCollection<FulfilmentLineRow> OrderLines { get; } = [];

    /// <summary>
    /// Gets the supplier confirmations of the selected order.
    /// </summary>
    public ObservableCollection<FulfilmentConfirmationRow> Confirmations { get; } = [];

    /// <summary>
    /// Gets the dispatches of the selected order.
    /// </summary>
    public ObservableCollection<FulfilmentDispatchRow> Dispatches { get; } = [];

    /// <summary>
    /// Gets the reminders of the selected order.
    /// </summary>
    public ObservableCollection<FulfilmentReminderRow> Reminders { get; } = [];

    /// <summary>
    /// Gets the editable lines of the supplier confirmation being entered.
    /// </summary>
    public ObservableCollection<ConfirmationEntryLine> ConfirmationLines { get; } = [];

    /// <summary>
    /// Gets the dispatch channels.
    /// </summary>
    public ObservableCollection<FulfilmentChoice> Channels { get; } = [];

    /// <summary>
    /// Gets the writes waiting to be sent.
    /// </summary>
    public ObservableCollection<FulfilmentQueueRow> PendingQueue { get; } = [];

    /// <summary>
    /// Gets the writes the server refused.
    /// </summary>
    public ObservableCollection<FulfilmentQueueRow> ConflictQueue { get; } = [];

    private CultureInfo Culture => _localization.CurrentCulture;

    /// <summary>
    /// Loads the workbench: replays the queue, then shows the overdue orders, the orders and the dropshipments.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            if (Channels.Count == 0)
            {
                foreach (var code in ProcurementCodes.Channels)
                {
                    Channels.Add(new FulfilmentChoice(code, _localization["Procurement_Channel_" + code]));
                }
            }

            _ = await _sender.Send(new FlushProcurementOutboxCommand());
            await ReloadAllAsync();
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
    private async Task RefreshAsync()
    {
        IsBusy = true;

        try
        {
            var flush = await _sender.Send(new FlushProcurementOutboxCommand());
            await ReloadAllAsync();
            StatusMessage = string.Format(Culture, _localization["Procurement_Flushed"], flush.Sent, flush.Conflicts, flush.Remaining);
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

    [RelayCommand(CanExecute = nameof(CanRemind))]
    private async Task RemindAsync()
    {
        if (SelectedOverdue is not { } overdue)
        {
            return;
        }

        var request = new OrderReminderRequest(Guid.NewGuid(), overdue.OrderId, overdue.Number, _time.GetUtcNow().UtcDateTime);

        _ = await RunWriteAsync(() => _sender.Send(new SendOrderReminderCommand(request)), reload: true);
    }

    private bool CanRemind() => SelectedOverdue is not null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanDispatch))]
    private async Task DispatchAsync()
    {
        if (SelectedOrder is not { } order)
        {
            return;
        }

        var request = new DispatchOrderRequest(
            Guid.NewGuid(),
            order.Id,
            order.Number,
            SelectedChannel,
            string.IsNullOrWhiteSpace(DispatchRecipient) ? null : DispatchRecipient.Trim(),
            _time.GetUtcNow().UtcDateTime);

        _ = await RunWriteAsync(() => _sender.Send(new DispatchPurchaseOrderCommand(request)), reload: true);
    }

    private bool CanDispatch() => SelectedOrder is { } order && ProcurementCodes.CanDispatch(order.Status) && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task RecordConfirmationAsync()
    {
        if (SelectedOrder is not { } order)
        {
            return;
        }

        List<ConfirmationLineRequest> lines = [];
        foreach (var entry in ConfirmationLines)
        {
            if (!entry.TryBuild(Culture, out var line))
            {
                StatusMessage = _localization["Procurement_InvalidNumber"];

                return;
            }

            if (line is not null)
            {
                lines.Add(line);
            }
        }

        var request = new SupplierConfirmationRequest(
            Guid.NewGuid(),
            order.Id,
            order.Number,
            ConfirmationNumber,
            ConfirmationDate is { } date ? DateOnly.FromDateTime(date) : null,
            PromisedDate is { } promised ? DateOnly.FromDateTime(promised) : null,
            string.IsNullOrWhiteSpace(ConfirmationNotes) ? null : ConfirmationNotes.Trim(),
            lines,
            _time.GetUtcNow().UtcDateTime);

        if (await RunWriteAsync(() => _sender.Send(new RecordSupplierConfirmationCommand(request)), reload: true))
        {
            ConfirmationNumber = string.Empty;
            ConfirmationNotes = string.Empty;
        }
    }

    private bool CanConfirm() => SelectedOrder is { } order && ProcurementCodes.CanConfirm(order.Status) && !IsBusy;

    [RelayCommand]
    private async Task FlushQueueAsync()
    {
        IsBusy = true;

        try
        {
            var flush = await _sender.Send(new FlushProcurementOutboxCommand());
            StatusMessage = string.Format(Culture, _localization["Procurement_Flushed"], flush.Sent, flush.Conflicts, flush.Remaining);
            await ShowQueueAsync();
            await ReloadAllAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DismissConflictAsync(FulfilmentQueueRow? row)
    {
        if (row is null)
        {
            return;
        }

        await _sender.Send(new DismissProcurementConflictCommand(row.Id));
        await ShowQueueAsync();
    }

    partial void OnSelectedOverdueChanged(FulfilmentOverdueRow? value) => RemindCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value)
    {
        RemindCommand.NotifyCanExecuteChanged();
        DispatchCommand.NotifyCanExecuteChanged();
        RecordConfirmationCommand.NotifyCanExecuteChanged();
        NotifyDropshipCommands();
    }

    partial void OnSelectedStatusFilterChanged(FulfilmentChoice? value)
    {
        if (!_suppressSelection)
        {
            _ = ReloadOrdersAsync();
        }
    }

    partial void OnSelectedOrderChanged(FulfilmentOrderRow? value)
    {
        DispatchCommand.NotifyCanExecuteChanged();
        RecordConfirmationCommand.NotifyCanExecuteChanged();

        if (!_suppressSelection)
        {
            _ = ShowOrderAsync(value);
        }
    }

    private async Task ReloadAllAsync()
    {
        await ReloadOverdueAsync();
        await ReloadOrdersAsync();
        await ReloadDropshipsAsync();
        await ShowQueueAsync();
    }

    private async Task ReloadOverdueAsync()
    {
        var load = await _sender.Send(new GetOverdueOrdersQuery());
        _overdueViews = load?.Data ?? [];
        Note(load?.FromCache, load?.SyncedAtUtc);

        OverdueOrders.Clear();
        foreach (var overdue in _overdueViews)
        {
            OverdueOrders.Add(ToOverdueRow(overdue));
        }

        SelectedOverdue = null;
    }

    private async Task ReloadOrdersAsync()
    {
        var keep = SelectedOrder?.Id;
        var load = await _sender.Send(new GetPurchaseOrdersQuery(SelectedStatusFilter?.Code));
        _orderViews = load?.Data ?? [];
        Note(load?.FromCache, load?.SyncedAtUtc);

        _suppressSelection = true;
        try
        {
            Orders.Clear();
            foreach (var order in _orderViews)
            {
                Orders.Add(ToOrderRow(order));
            }

            SelectedOrder = keep is { } id ? Orders.FirstOrDefault(row => row.Id == id) : null;
        }
        finally
        {
            _suppressSelection = false;
        }

        await ShowOrderAsync(SelectedOrder);
    }

    private async Task ShowOrderAsync(FulfilmentOrderRow? row)
    {
        OrderLines.Clear();
        Confirmations.Clear();
        Dispatches.Clear();
        Reminders.Clear();
        ConfirmationLines.Clear();
        HasSelectedOrder = row is not null;

        if (row is null)
        {
            SelectedOrderTitle = string.Empty;
            SelectedOrderSummary = string.Empty;

            return;
        }

        SelectedOrderTitle = row.Number + " - " + row.SupplierName;
        SelectedOrderSummary = row.StatusText + " | " + row.PromisedText;

        var detail = await _sender.Send(new GetPurchaseOrderQuery(row.Id));
        var view = detail?.Data ?? _orderViews.FirstOrDefault(order => order.Id == row.Id);
        if (view is not null)
        {
            foreach (var line in view.Lines)
            {
                OrderLines.Add(ToLineRow(line, view.Currency));
                ConfirmationLines.Add(new ConfirmationEntryLine(line.Id, LineLabel(line), FormatQuantity(line.QuantityOrdered, line.Uom), line.QuantityOrdered, line.UnitPrice));
            }

            PromisedDate = view.PromisedDeliveryDate is { } day ? day.ToDateTime(TimeOnly.MinValue) : null;
        }

        ConfirmationDate = _time.GetLocalNow().DateTime.Date;

        foreach (var confirmation in (await _sender.Send(new GetOrderConfirmationsQuery(row.Id)))?.Data ?? [])
        {
            Confirmations.Add(ToConfirmationRow(confirmation));
        }

        foreach (var dispatch in (await _sender.Send(new GetOrderDispatchesQuery(row.Id)))?.Data ?? [])
        {
            Dispatches.Add(new FulfilmentDispatchRow(
                Channel(dispatch.Channel),
                dispatch.Recipient ?? RebateFormat.NoValue,
                dispatch.Status,
                dispatch.DispatchedAt.ToLocalTime().ToString(DateTimeFormat, Culture)));
        }

        foreach (var reminder in (await _sender.Send(new GetOrderRemindersQuery(row.Id)))?.Data ?? [])
        {
            Reminders.Add(new FulfilmentReminderRow(
                reminder.Level.ToString(Culture),
                Channel(reminder.Channel),
                reminder.DaysOverdue.ToString(Culture),
                reminder.SentAt.ToLocalTime().ToString(DateTimeFormat, Culture)));
        }
    }

    private async Task ShowQueueAsync()
    {
        var queue = await _sender.Send(new GetProcurementQueueQuery());
        PendingQueue.Clear();
        ConflictQueue.Clear();
        foreach (var action in queue.Pending)
        {
            PendingQueue.Add(ToQueueRow(action));
        }

        foreach (var action in queue.Conflicts)
        {
            ConflictQueue.Add(ToQueueRow(action));
        }

        QueueText = string.Format(Culture, _localization["Procurement_QueueFormat"], queue.Pending.Count, queue.Conflicts.Count);
    }

    /// <summary>
    /// Runs a write and shows its outcome.
    /// </summary>
    /// <param name="write">The write.</param>
    /// <param name="reload">Whether to reload the lists once the write was accepted.</param>
    /// <returns>True when the server accepted the write or it was queued.</returns>
    private async Task<bool> RunWriteAsync(Func<Task<ProcurementActionResult>> write, bool reload)
    {
        IsBusy = true;

        try
        {
            var result = await write();
            StatusMessage = Describe(result);
            if (result.IsAccepted && reload)
            {
                await ReloadAllAsync();
            }
            else
            {
                await ShowQueueAsync();
            }

            return result.IsAccepted;
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string Describe(ProcurementActionResult result) => result.Status switch
    {
        ProcurementActionStatus.Sent => _localization["Procurement_Done_Sent"],
        ProcurementActionStatus.Queued => _localization["Procurement_Done_Queued"],
        ProcurementActionStatus.Conflict => string.Format(Culture, _localization["Procurement_Done_Conflict"], result.Detail ?? string.Empty),
        ProcurementActionStatus.Rejected => result.ErrorKey is { } key ? _localization[key] : _localization["Procurement_Err_Lines"],
        _ => _localization["Procurement_Err_Lines"],
    };

    private void Note(bool? fromCache, DateTime? syncedAtUtc)
    {
        if (fromCache is null || syncedAtUtc is null)
        {
            IsOffline = true;

            return;
        }

        IsOffline = fromCache.Value;
        LastSyncedText = string.Format(Culture, _localization["Procurement_Synced"], syncedAtUtc.Value.ToLocalTime().ToString(DateTimeFormat, Culture));
    }

    private string LineLabel(PurchaseOrderLineView line) => line.LineNumber.ToString(Culture) + " - " + line.Description;

    private string FormatQuantity(decimal quantity, string uom) =>
        string.Format(Culture, _localization["Procurement_QuantityFormat"], quantity.ToString("0.####", Culture), uom).Trim();

    private string Channel(string code) => _localization["Procurement_Channel_" + code];

    private FulfilmentOverdueRow ToOverdueRow(OverdueOrderView overdue) => new(
        overdue.Order.Id,
        overdue.Order.Number,
        overdue.Order.SupplierName,
        RebateFormat.Date(overdue.Order.PromisedDeliveryDate ?? overdue.Order.ExpectedDeliveryDate, Culture),
        overdue.DaysOverdue.ToString(Culture),
        overdue.ReminderCount.ToString(Culture),
        overdue.LastReminderAt is { } last ? last.ToLocalTime().ToString(DateTimeFormat, Culture) : RebateFormat.NoValue,
        _localization[overdue.NextReminderDue ? "Procurement_Yes" : "Procurement_No"],
        overdue.NextReminderDue);

    private FulfilmentOrderRow ToOrderRow(PurchaseOrderView order) => new(
        order.Id,
        order.Number,
        order.SupplierName,
        order.Status,
        _localization["Procurement_Status_" + order.Status],
        RebateFormat.Date(order.PromisedDeliveryDate, Culture),
        RebateFormat.Date(order.ExpectedDeliveryDate, Culture),
        RebateFormat.Money(order.TotalAmount, order.Currency, Culture),
        order.ReminderCount.ToString(Culture),
        order.IsDropship ? _localization["Procurement_Yes"] : string.Empty,
        order.SupplierConfirmationNumber ?? RebateFormat.NoValue);

    private FulfilmentLineRow ToLineRow(PurchaseOrderLineView line, string currency) => new(
        line.Id,
        line.LineNumber,
        line.Description,
        FormatQuantity(line.QuantityOrdered, line.Uom),
        ProcurementDisplay.BaseQuantity(line, _localization["Procurement_BaseFormat"], Culture),
        FormatQuantity(line.QuantityReceived, line.Uom),
        RebateFormat.Money(line.UnitPrice, currency, Culture),
        line.DiscountCascade.Count == 0 ? RebateFormat.NoValue : PurchasePricingCalculator.FormatCascade(line.DiscountCascade, Culture),
        RebateFormat.Percent(line.DiscountPercentEffective, Culture),
        RebateFormat.Date(line.PromisedDeliveryDate, Culture),
        RebateFormat.Money(line.LineNetAmount, currency, Culture));

    private FulfilmentConfirmationRow ToConfirmationRow(SupplierConfirmationView confirmation)
    {
        var late = confirmation.Lines.Where(line => line.QuantityVariance != 0m || line.PriceVariance != 0m || line.DeliveryDelayDays > 0).Select(line =>
            string.Format(
                Culture,
                _localization["Procurement_VarianceLine"],
                line.QuantityVariance.ToString("+0.####;-0.####;0", Culture),
                line.PriceVariancePercent.ToString("+0.##;-0.##;0", Culture),
                line.DeliveryDelayDays.ToString(Culture)));

        return new FulfilmentConfirmationRow(
            confirmation.ConfirmationNumber,
            RebateFormat.Date(confirmation.ConfirmedAt, Culture),
            confirmation.HasVariance ? _localization["Procurement_HasVariance"] : _localization["Procurement_NoVariance"],
            confirmation.RequiresReview ? _localization["Procurement_NeedsReview"] : string.Empty,
            string.Join(" | ", late));
    }

    private FulfilmentQueueRow ToQueueRow(ProcurementPendingAction action) => new(
        action.Id,
        _localization["Procurement_Action_" + action.Action],
        action.Reference,
        action.OccurredAtUtc.ToLocalTime().ToString(DateTimeFormat, Culture),
        action.Error ?? string.Empty);
}
