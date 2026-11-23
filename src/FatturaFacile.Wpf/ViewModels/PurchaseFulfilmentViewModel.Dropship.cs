// <copyright file="PurchaseFulfilmentViewModel.Dropship.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Procurement;
using FatturaFacile.Application.Procurement;

namespace FatturaFacile.Wpf.ViewModels;

internal sealed partial class PurchaseFulfilmentViewModel
{
    private IReadOnlyList<DropshipmentView> _dropViews = [];

    [ObservableProperty]
    private FulfilmentChoice? _selectedDropshipFilter;

    [ObservableProperty]
    private FulfilmentDropshipRow? _selectedDropship;

    [ObservableProperty]
    private bool _hasSelectedDropship;

    [ObservableProperty]
    private string _dropshipSummary = string.Empty;

    [ObservableProperty]
    private string _ddtCarrier = string.Empty;

    [ObservableProperty]
    private string _ddtTracking = string.Empty;

    [ObservableProperty]
    private string _ddtPackagesText = string.Empty;

    [ObservableProperty]
    private string _ddtWeightText = string.Empty;

    [ObservableProperty]
    private string _ddtTransportBy = string.Empty;

    [ObservableProperty]
    private DateTime? _ddtShipmentDate;

    [ObservableProperty]
    private DateTime? _deliveryDate;

    [ObservableProperty]
    private string _deliveryProof = string.Empty;

    [ObservableProperty]
    private string _deliverySupplierReference = string.Empty;

    [ObservableProperty]
    private string _createSalesOrderIdText = string.Empty;

    [ObservableProperty]
    private string _createSupplierIdText = string.Empty;

    [ObservableProperty]
    private string _createName = string.Empty;

    [ObservableProperty]
    private string _createStreet = string.Empty;

    [ObservableProperty]
    private string _createHouseNumber = string.Empty;

    [ObservableProperty]
    private string _createPostalCode = string.Empty;

    [ObservableProperty]
    private string _createCity = string.Empty;

    [ObservableProperty]
    private string _createCountry = string.Empty;

    [ObservableProperty]
    private string _createNotes = string.Empty;

    /// <summary>
    /// Gets the status filters of the dropshipment list.
    /// </summary>
    public ObservableCollection<FulfilmentChoice> DropshipFilters { get; } = [];

    /// <summary>
    /// Gets the dropshipments.
    /// </summary>
    public ObservableCollection<FulfilmentDropshipRow> Dropships { get; } = [];

    /// <summary>
    /// Gets the virtual stock movements of the selected dropshipment.
    /// </summary>
    public ObservableCollection<FulfilmentMovementRow> Movements { get; } = [];

    /// <summary>
    /// Gets the editable lines of the delivery notice being entered.
    /// </summary>
    public ObservableCollection<DeliveryEntryLine> DeliveryLines { get; } = [];

    private static string? Optional(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    [RelayCommand(CanExecute = nameof(CanIssueDdt))]
    private async Task IssueDdtAsync()
    {
        if (SelectedDropship is not { } drop)
        {
            return;
        }

        if (!ProcurementInput.TryDecimal(DdtPackagesText, Culture, out var packages) || !ProcurementInput.TryDecimal(DdtWeightText, Culture, out var weight))
        {
            StatusMessage = _localization["Procurement_InvalidNumber"];

            return;
        }

        var request = new IssueDdtRequest(
            Guid.NewGuid(),
            drop.Id,
            drop.RecipientText,
            Optional(DdtCarrier),
            Optional(DdtTracking),
            packages is { } count ? (int)count : null,
            weight,
            Optional(DdtTransportBy),
            DdtShipmentDate is { } shipped ? DateOnly.FromDateTime(shipped) : null,
            _time.GetUtcNow().UtcDateTime);

        _ = await RunWriteAsync(() => _sender.Send(new IssueDropshipDdtCommand(request)), reload: true);
    }

    private bool CanIssueDdt() => SelectedDropship is { } drop && ProcurementCodes.CanIssueDdt(drop.Status) && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanDeliver))]
    private async Task RecordDeliveryAsync()
    {
        if (SelectedDropship is not { } drop)
        {
            return;
        }

        List<DeliveryLineRequest> lines = [];
        foreach (var entry in DeliveryLines)
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

        var request = new RecordDeliveryRequest(
            Guid.NewGuid(),
            drop.Id,
            drop.RecipientText,
            DeliveryDate is { } delivered ? DateOnly.FromDateTime(delivered) : null,
            Optional(DeliveryProof),
            Optional(DeliverySupplierReference),
            lines,
            _time.GetUtcNow().UtcDateTime);

        _ = await RunWriteAsync(() => _sender.Send(new RecordDropshipDeliveryCommand(request)), reload: true);
    }

    private bool CanDeliver() => SelectedDropship is { } drop && ProcurementCodes.CanDeliver(drop.Status) && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanCreateDropship))]
    private async Task CreateDropshipAsync()
    {
        _ = Guid.TryParse(CreateSalesOrderIdText.Trim(), out var salesOrderId);
        _ = Guid.TryParse(CreateSupplierIdText.Trim(), out var supplierId);

        var address = new DropshipAddress(
            Name: Optional(CreateName),
            Street: CreateStreet.Trim(),
            HouseNumber: Optional(CreateHouseNumber),
            PostalCode: CreatePostalCode.Trim(),
            City: CreateCity.Trim(),
            Region: null,
            CountryCode: CreateCountry.Trim(),
            Phone: null,
            Email: null,
            DeliveryNotes: null);
        var request = new CreateDropshipmentRequest(
            CommandId: Guid.NewGuid(),
            Reference: CreateSalesOrderIdText.Trim(),
            SalesOrderId: salesOrderId,
            SupplierId: supplierId,
            ExpectedDeliveryDate: null,
            Currency: null,
            Notes: Optional(CreateNotes),
            Address: address,
            Lines: [],
            OccurredAtUtc: _time.GetUtcNow().UtcDateTime);

        if (await RunWriteAsync(() => _sender.Send(new CreateDropshipmentCommand(request)), reload: true))
        {
            CreateSalesOrderIdText = string.Empty;
            CreateNotes = string.Empty;
        }
    }

    private bool CanCreateDropship() => !IsBusy && !string.IsNullOrWhiteSpace(CreateSalesOrderIdText) && !string.IsNullOrWhiteSpace(CreateSupplierIdText);

    partial void OnCreateSalesOrderIdTextChanged(string value) => CreateDropshipCommand.NotifyCanExecuteChanged();

    partial void OnCreateSupplierIdTextChanged(string value) => CreateDropshipCommand.NotifyCanExecuteChanged();

    partial void OnSelectedDropshipFilterChanged(FulfilmentChoice? value)
    {
        if (!_suppressSelection)
        {
            _ = ReloadDropshipsAsync();
        }
    }

    partial void OnSelectedDropshipChanged(FulfilmentDropshipRow? value)
    {
        NotifyDropshipCommands();

        if (!_suppressSelection)
        {
            _ = ShowDropshipAsync(value);
        }
    }

    private void NotifyDropshipCommands()
    {
        IssueDdtCommand.NotifyCanExecuteChanged();
        RecordDeliveryCommand.NotifyCanExecuteChanged();
        CreateDropshipCommand.NotifyCanExecuteChanged();
    }

    private async Task ReloadDropshipsAsync()
    {
        if (DropshipFilters.Count == 0)
        {
            DropshipFilters.Add(new FulfilmentChoice(Code: null, Label: _localization["Procurement_AllStatuses"]));
            foreach (var code in ProcurementCodes.DropshipStatuses)
            {
                DropshipFilters.Add(new FulfilmentChoice(code, _localization["Procurement_Dropship_" + code]));
            }

            _suppressSelection = true;
            SelectedDropshipFilter = DropshipFilters[0];
            _suppressSelection = false;
        }

        var keep = SelectedDropship?.Id;
        var load = await _sender.Send(new GetDropshipmentsQuery(SelectedDropshipFilter?.Code));
        _dropViews = load?.Data ?? [];
        Note(load?.FromCache, load?.SyncedAtUtc);

        _suppressSelection = true;
        try
        {
            Dropships.Clear();
            foreach (var drop in _dropViews)
            {
                Dropships.Add(ToDropshipRow(drop));
            }

            SelectedDropship = keep is { } id ? Dropships.FirstOrDefault(row => row.Id == id) : null;
        }
        finally
        {
            _suppressSelection = false;
        }

        await ShowDropshipAsync(SelectedDropship);
    }

    private async Task ShowDropshipAsync(FulfilmentDropshipRow? row)
    {
        Movements.Clear();
        DeliveryLines.Clear();
        HasSelectedDropship = row is not null;
        DropshipSummary = row is null ? string.Empty : row.StatusText + " | " + row.RecipientText;

        if (row is null)
        {
            return;
        }

        var detail = await _sender.Send(new GetDropshipmentQuery(row.Id));
        var view = detail?.Data ?? _dropViews.FirstOrDefault(drop => drop.Id == row.Id);
        if (view is not null)
        {
            foreach (var movement in view.Movements)
            {
                Movements.Add(new FulfilmentMovementRow(
                    movement.Direction,
                    movement.QuantityBase.ToString("0.####", Culture) + " " + movement.BaseUom,
                    RebateFormat.Money(movement.ValuedAmount, string.Empty, Culture).Trim(),
                    movement.MovedAt.ToLocalTime().ToString(DateTimeFormat, Culture)));
            }
        }

        var order = await _sender.Send(new GetPurchaseOrderQuery(row.PurchaseOrderId));
        foreach (var line in order?.Data.Lines ?? [])
        {
            var open = Math.Max(0m, line.QuantityOrdered - line.QuantityReceived);
            DeliveryLines.Add(new DeliveryEntryLine(line.Id, LineLabel(line), FormatQuantity(open, line.Uom)));
        }
    }

    private FulfilmentDropshipRow ToDropshipRow(DropshipmentView drop) => new(
        drop.Id,
        drop.PurchaseOrderId,
        drop.Status,
        _localization["Procurement_Dropship_" + drop.Status],
        drop.RecipientName.Length == 0 ? drop.RecipientAddress : drop.RecipientName + ", " + drop.RecipientAddress,
        drop.DdtNumber ?? RebateFormat.NoValue,
        drop.BillingAuthorized ? drop.BillingAuthorizationReference ?? _localization["Procurement_Yes"] : _localization["Procurement_No"],
        drop.DeliveredAt is { } at ? at.ToLocalTime().ToString(DateTimeFormat, Culture) : RebateFormat.NoValue);
}
