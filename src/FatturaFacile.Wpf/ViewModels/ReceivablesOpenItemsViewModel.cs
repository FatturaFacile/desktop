// <copyright file="ReceivablesOpenItemsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The open items list of the receivables: filters, the drill-down from an aging bucket, the allocations of the selected item,
/// the reconciliation dialog, the dispute and the reversal of an allocation. It works from the cache offline.
/// </summary>
internal sealed partial class ReceivablesOpenItemsViewModel : ObservableObject
{
    private const int PerPage = 50;
    private const string AnyKey = "";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;
    private readonly Func<Task>? _changed;

    private Guid? _customerId;
    private string? _currency;

    [ObservableProperty]
    private ReceivablesChoice? _selectedStatus;

    [ObservableProperty]
    private string _documentType = string.Empty;

    [ObservableProperty]
    private DateTime? _dueFrom;

    [ObservableProperty]
    private DateTime? _dueTo;

    [ObservableProperty]
    private ReceivablesChoice? _selectedBucket;

    [ObservableProperty]
    private bool _openOnly = true;

    [ObservableProperty]
    private int _page = 1;

    [ObservableProperty]
    private int _lastPage = 1;

    [ObservableProperty]
    private string _pageText = string.Empty;

    [ObservableProperty]
    private ReceivablesItemRow? _selectedRow;

    [ObservableProperty]
    private bool _hasSelection;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _noticeText = string.Empty;

    [ObservableProperty]
    private ReceivablesReconcileViewModel? _reconcile;

    [ObservableProperty]
    private bool _isReconcileOpen;

    [ObservableProperty]
    private string _disputeReason = string.Empty;

    [ObservableProperty]
    private bool _isDisputeOpen;

    [ObservableProperty]
    private string _actionText = string.Empty;

    [ObservableProperty]
    private string _actionState = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesOpenItemsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    /// <param name="changed">Called after an allocation, a reversal or a dispute, to refresh every view.</param>
    public ReceivablesOpenItemsViewModel(ISender sender, ILocalizationService localization, TimeProvider time, Func<Task>? changed)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        _changed = changed;

        StatusChoices =
        [
            new ReceivablesChoice(AnyKey, _localization["Receivables_All"]),
            .. new[] { ReceivableStatus.Open, ReceivableStatus.PartiallyPaid, ReceivableStatus.Overdue, ReceivableStatus.Disputed, ReceivableStatus.Paid, ReceivableStatus.WrittenOff }
                .Select(status => new ReceivablesChoice(status.ToWire(), _localization["Receivables_Status_" + status.ToWire()])),
        ];
        _selectedStatus = StatusChoices[0];
        BucketChoices.Add(new ReceivablesChoice(AnyKey, _localization["Receivables_All"]));
        _selectedBucket = BucketChoices[0];
    }

    /// <summary>
    /// Gets the status filter entries.
    /// </summary>
    public IReadOnlyList<ReceivablesChoice> StatusChoices { get; }

    /// <summary>
    /// Gets the aging bucket filter entries: all, then the buckets of the aging.
    /// </summary>
    public ObservableCollection<ReceivablesChoice> BucketChoices { get; } = [];

    /// <summary>
    /// Gets the items of the page.
    /// </summary>
    public ObservableCollection<ReceivablesItemRow> Rows { get; } = [];

    /// <summary>
    /// Gets the allocations of the selected item.
    /// </summary>
    public ObservableCollection<ReceivablesAllocationRow> Allocations { get; } = [];

    /// <summary>
    /// Replaces the bucket filter entries with the buckets the server reported.
    /// </summary>
    /// <param name="buckets">The buckets.</param>
    public void SetBuckets(IReadOnlyList<AgingBucket> buckets)
    {
        ArgumentNullException.ThrowIfNull(buckets);

        var keep = SelectedBucket?.Key ?? AnyKey;
        BucketChoices.Clear();
        BucketChoices.Add(new ReceivablesChoice(AnyKey, _localization["Receivables_All"]));
        foreach (var bucket in buckets)
        {
            BucketChoices.Add(new ReceivablesChoice(bucket.Key, ReceivablesFormat.BucketLabel(bucket, _localization)));
        }

        SelectedBucket = BucketChoices.FirstOrDefault(choice => string.Equals(choice.Key, keep, StringComparison.Ordinal)) ?? BucketChoices[0];
    }

    /// <summary>
    /// Loads the first page for a customer and a currency.
    /// </summary>
    /// <param name="customerId">The customer, null for everybody.</param>
    /// <param name="currency">The ISO currency, null for the company currency.</param>
    /// <returns>A task representing the load.</returns>
    public Task LoadAsync(Guid? customerId, string? currency)
    {
        _customerId = customerId;
        _currency = currency;
        Page = 1;

        return FetchAsync();
    }

    /// <summary>
    /// Shows the items behind an aging figure: the open items of a bucket, for one customer or for everybody.
    /// </summary>
    /// <param name="customerId">The customer, null for everybody.</param>
    /// <param name="bucketKey">The bucket key, null for every bucket.</param>
    /// <returns>A task representing the load.</returns>
    public Task DrillDownAsync(Guid? customerId, string? bucketKey)
    {
        _customerId = customerId;
        OpenOnly = true;
        SelectedStatus = StatusChoices[0];
        SelectedBucket = BucketChoices.FirstOrDefault(choice => string.Equals(choice.Key, bucketKey ?? AnyKey, StringComparison.Ordinal)) ?? BucketChoices[0];
        Page = 1;

        return FetchAsync();
    }

    partial void OnSelectedRowChanged(ReceivablesItemRow? value)
    {
        HasSelection = value is not null;
        Reconcile = null;
        IsReconcileOpen = false;
        IsDisputeOpen = false;
        Allocations.Clear();
        if (value is not null)
        {
            _ = LoadDetailAsync(value.Item.Id);
        }
    }

    partial void OnReconcileChanged(ReceivablesReconcileViewModel? value) => IsReconcileOpen = value is not null;

    [RelayCommand]
    private Task RefreshAsync() => FetchAsync();

    [RelayCommand]
    private Task ApplyFiltersAsync()
    {
        Page = 1;

        return FetchAsync();
    }

    [RelayCommand]
    private Task NextPageAsync()
    {
        if (Page >= LastPage)
        {
            return Task.CompletedTask;
        }

        Page++;

        return FetchAsync();
    }

    [RelayCommand]
    private Task PreviousPageAsync()
    {
        if (Page <= 1)
        {
            return Task.CompletedTask;
        }

        Page--;

        return FetchAsync();
    }

    [RelayCommand]
    private async Task BeginReconcileAsync()
    {
        if (SelectedRow?.Item is not { } item)
        {
            return;
        }

        try
        {
            var payments = await _sender.Send(new GetUnreconciledPaymentsQuery(item.CustomerId));
            Reconcile = new ReceivablesReconcileViewModel(_sender, _localization, _time, item, payments?.Data ?? [], _changed);
        }
        catch (UnauthorizedAccessException ex)
        {
            ActionText = ex.Message;
            ActionState = ReceivablesFormat.Bad;
        }
    }

    [RelayCommand]
    private void CloseReconcile() => Reconcile = null;

    [RelayCommand]
    private void BeginDispute()
    {
        DisputeReason = SelectedRow?.Item.DisputeReason ?? string.Empty;
        IsDisputeOpen = SelectedRow is not null;
    }

    [RelayCommand]
    private void CloseDispute() => IsDisputeOpen = false;

    [RelayCommand]
    private Task MarkDisputedAsync() => SetDisputeAsync(disputed: true);

    [RelayCommand]
    private Task ClearDisputeAsync() => SetDisputeAsync(disputed: false);

    [RelayCommand]
    private async Task ReverseAsync(ReceivablesAllocationRow? row)
    {
        if (row is null || SelectedRow?.Item is not { } item)
        {
            return;
        }

        try
        {
            ReverseAllocationRequest request = new(Guid.NewGuid(), item.CustomerId, item.DocumentNumber, row.Allocation.Id, _time.GetUtcNow().UtcDateTime);
            var result = await _sender.Send(new ReverseReceivableAllocationCommand(request));
            await ShowResultAsync(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            ActionText = ex.Message;
            ActionState = ReceivablesFormat.Bad;
        }
    }

    private static string StateOf(ReceivableItem item) => item.Status switch
    {
        ReceivableStatus.Overdue => "Overdue",
        ReceivableStatus.Disputed => "Disputed",
        ReceivableStatus.Paid or ReceivableStatus.WrittenOff => "Closed",
        ReceivableStatus.PartiallyPaid => "Partial",
        _ => "Open",
    };

    private static DateOnly? Day(DateTime? value) => value is { } instant ? DateOnly.FromDateTime(instant) : null;

    private async Task SetDisputeAsync(bool disputed)
    {
        if (SelectedRow?.Item is not { } item)
        {
            return;
        }

        try
        {
            DisputeRequest request = new(Guid.NewGuid(), item.CustomerId, item.DocumentNumber, item.Id, disputed, disputed ? DisputeReason : null, _time.GetUtcNow().UtcDateTime);
            var result = await _sender.Send(new SetReceivableDisputeCommand(request));
            await ShowResultAsync(result);
            if (result.IsAccepted)
            {
                IsDisputeOpen = false;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            ActionText = ex.Message;
            ActionState = ReceivablesFormat.Bad;
        }
    }

    private async Task ShowResultAsync(ReceivablesActionResult result)
    {
        var (text, state) = ReceivablesFormat.Describe(result, _localization);
        ActionText = text;
        ActionState = state;

        if (result.IsAccepted && _changed is not null)
        {
            await _changed();
        }
    }

    private async Task FetchAsync()
    {
        IsBusy = true;

        try
        {
            var statusKey = SelectedStatus?.Key ?? AnyKey;
            var bucketKey = SelectedBucket?.Key ?? AnyKey;
            ReceivableItemFilter filter = new(
                _customerId,
                statusKey.Length == 0 ? null : ReceivablesWire.ParseStatus(statusKey),
                string.IsNullOrWhiteSpace(DocumentType) ? null : DocumentType.Trim(),
                _currency,
                Day(DueFrom),
                Day(DueTo),
                null,
                null,
                null,
                null,
                bucketKey.Length == 0 ? null : bucketKey,
                OpenOnly,
                Math.Max(1, Page),
                PerPage);
            var load = await _sender.Send(new GetReceivableItemsQuery(filter));
            Apply(load);
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

    private void Apply(ReceivablesLoad<ReceivableItemPage>? load)
    {
        var keep = SelectedRow?.Item.Id;
        Rows.Clear();

        if (load is null)
        {
            StatusMessage = _localization["Receivables_NoData"];
            NoticeText = string.Empty;
            PageText = string.Empty;
            LastPage = 1;

            return;
        }

        var culture = _localization.CurrentCulture;
        foreach (var item in load.Data.Items)
        {
            Rows.Add(new ReceivablesItemRow(
                item,
                item.DocumentNumber,
                ReceivablesFormat.Day(item.IssueDate, culture),
                ReceivablesFormat.Day(item.DueDate, culture),
                ReceivablesFormat.Money(item.OriginalAmount, culture),
                ReceivablesFormat.Money(item.OpenAmount, culture),
                _localization["Receivables_Status_" + item.Status.ToWire()],
                StateOf(item),
                item.DaysPastDue > 0 ? item.DaysPastDue.ToString(culture) : "-"));
        }

        LastPage = Math.Max(1, load.Data.LastPage);
        PageText = string.Format(culture, _localization["Receivables_PageOf"], load.Data.CurrentPage.ToString(culture), LastPage.ToString(culture), load.Data.Total.ToString(culture));
        NoticeText = load.FromCache ? _localization["Receivables_OfflineCache"] : string.Empty;
        StatusMessage = Rows.Count == 0 ? _localization["Receivables_NoItems"] : string.Empty;
        SelectedRow = keep is { } id ? Rows.FirstOrDefault(row => row.Item.Id == id) : null;
    }

    private async Task LoadDetailAsync(Guid itemId)
    {
        try
        {
            var load = await _sender.Send(new GetReceivableItemQuery(itemId));

            // The selection moved on while the answer was on its way.
            if (SelectedRow?.Item.Id != itemId)
            {
                return;
            }

            Allocations.Clear();
            var culture = _localization.CurrentCulture;
            foreach (var allocation in load?.Data.Allocations ?? [])
            {
                Allocations.Add(new ReceivablesAllocationRow(
                    allocation,
                    _localization["Receivables_Type_" + allocation.Type.ToWire()],
                    ReceivablesFormat.Money(allocation.Amount, culture),
                    ReceivablesFormat.Day(allocation.ValueDate, culture),
                    allocation.SourceReference ?? allocation.Notes ?? string.Empty,
                    allocation.IsReversed ? _localization["Receivables_Reversed"] : string.Empty,
                    !allocation.IsReversed));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            ActionText = ex.Message;
            ActionState = ReceivablesFormat.Bad;
        }
    }
}
