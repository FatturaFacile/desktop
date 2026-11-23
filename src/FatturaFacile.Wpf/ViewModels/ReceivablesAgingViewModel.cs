// <copyright file="ReceivablesAgingViewModel.cs" company="Fattura Facile">
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
/// The aging of the receivables: the buckets the server reports, rendered as they come, the aging of every customer and the
/// drill-down from a figure to the open items behind it.
/// </summary>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localization">The localization service.</param>
/// <param name="drillDown">Shows the items of a bucket (bucket key, customer).</param>
internal sealed partial class ReceivablesAgingViewModel(ISender sender, ILocalizationService localization, Func<string?, Guid?, Task> drillDown) : ObservableObject
{
    private string? _currency;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _noticeText = string.Empty;

    [ObservableProperty]
    private string _totalOpenText = string.Empty;

    [ObservableProperty]
    private string _totalOverdueText = string.Empty;

    [ObservableProperty]
    private string _over90Text = string.Empty;

    [ObservableProperty]
    private string _currencyText = string.Empty;

    /// <summary>
    /// Gets the buckets of the last answer, in aging order.
    /// </summary>
    public IReadOnlyList<AgingBucket> Buckets { get; private set; } = new AgingBucketEngine().Buckets;

    /// <summary>
    /// Gets the names of the customers in the aging.
    /// </summary>
    public IReadOnlyDictionary<Guid, string> CustomerNames { get; private set; } = new Dictionary<Guid, string>();

    /// <summary>
    /// Gets the company figures per bucket.
    /// </summary>
    public ObservableCollection<ReceivablesAgingCell> Cells { get; } = [];

    /// <summary>
    /// Gets the aging of every customer.
    /// </summary>
    public ObservableCollection<ReceivablesAgingRow> Customers { get; } = [];

    /// <summary>
    /// Loads the aging.
    /// </summary>
    /// <param name="currency">The ISO currency, null for the company currency.</param>
    /// <returns>A task representing the load.</returns>
    public async Task LoadAsync(string? currency)
    {
        _currency = currency;
        IsBusy = true;

        try
        {
            var load = await sender.Send(new GetReceivablesAgingQuery(CustomerId: null, currency));
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

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync(_currency);

    [RelayCommand]
    private Task DrillDownAsync(ReceivablesAgingCell? cell) => cell is null ? Task.CompletedTask : drillDown(cell.Key, cell.CustomerId);

    private void Apply(ReceivablesLoad<AgingReport>? load)
    {
        Cells.Clear();
        Customers.Clear();

        if (load is null)
        {
            StatusMessage = localization["Receivables_NoData"];
            NoticeText = string.Empty;
            TotalOpenText = string.Empty;
            TotalOverdueText = string.Empty;
            Over90Text = string.Empty;

            return;
        }

        var report = load.Data;
        var culture = localization.CurrentCulture;
        Buckets = report.Buckets;
        CustomerNames = report.Customers.ToDictionary(row => row.CustomerId, row => row.CustomerName);
        CurrencyText = report.Currency;
        TotalOpenText = ReceivablesFormat.Money(report.TotalOpen, culture);
        TotalOverdueText = ReceivablesFormat.Money(report.TotalOverdue, culture);
        Over90Text = ReceivablesFormat.Money(report.Over90, culture);
        NoticeText = load.FromCache ? localization["Receivables_OfflineCache"] : string.Empty;
        StatusMessage = report.Customers.Count == 0 && report.TotalOpen == 0m ? localization["Receivables_NoItems"] : string.Empty;

        foreach (var bucket in report.Buckets)
        {
            var amount = report.Totals.GetValueOrDefault(bucket.Key);
            var share = report.TotalOpen > 0m ? amount / report.TotalOpen * 100m : 0m;
            Cells.Add(new ReceivablesAgingCell(bucket.Key, CustomerId: null, ReceivablesFormat.BucketLabel(bucket, localization), ReceivablesFormat.Money(amount, culture), ReceivablesFormat.Percent(share, culture, string.Empty)));
        }

        foreach (var row in report.Customers)
        {
            Customers.Add(new ReceivablesAgingRow(
                row.CustomerId,
                row.CustomerName,
                ReceivablesFormat.Money(row.TotalOpen, culture),
                ReceivablesFormat.Money(row.TotalOverdue, culture),
                [.. report.Buckets.Select(bucket => new ReceivablesAgingCell(bucket.Key, row.CustomerId, ReceivablesFormat.BucketLabel(bucket, localization), ReceivablesFormat.Money(row.Buckets.GetValueOrDefault(bucket.Key), culture), string.Empty))]));
        }
    }
}
