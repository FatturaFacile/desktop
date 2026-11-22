// <copyright file="FieldWorkOrderViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the work order view: the work order picker, the offline bundle of a technician, the parts taken from the van with the
/// covered and the chargeable ones told apart, the service report viewer with its PDF and the convert-to-invoice action, and the
/// forms of the technician (status, part consumption, signed service report). The technician operations are queued in the local
/// outbox, so they work offline; the work order shows them as pending until the server confirms and prices them.
/// </summary>
internal sealed partial class FieldWorkOrderViewModel : ObservableObject
{
    private const string NoValue = "-";

    private readonly ISender _sender;
    private readonly FieldServiceLabels _labels;

    private FieldServiceWorkOrder? _order;
    private string? _formOrderId;
    private bool _suppressSelection;

    [ObservableProperty]
    private FieldWorkOrderRow? _selectedWorkOrder;

    [ObservableProperty]
    private FieldServiceChoice<string>? _selectedTechnician;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _hasOrder;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private string _numberText = NoValue;

    [ObservableProperty]
    private string _titleText = NoValue;

    [ObservableProperty]
    private string _statusText = NoValue;

    [ObservableProperty]
    private string _typeText = NoValue;

    [ObservableProperty]
    private string _priorityText = NoValue;

    [ObservableProperty]
    private string _problemText = string.Empty;

    [ObservableProperty]
    private string _windowText = NoValue;

    [ObservableProperty]
    private string _siteText = NoValue;

    [ObservableProperty]
    private string _skillsText = NoValue;

    [ObservableProperty]
    private bool _hasPendingOperations;

    [ObservableProperty]
    private bool _canAdvance;

    [ObservableProperty]
    private bool _canSign;

    [ObservableProperty]
    private bool _hasReport;

    [ObservableProperty]
    private bool _reportIsPending;

    [ObservableProperty]
    private string _reportNumberText = NoValue;

    [ObservableProperty]
    private string _reportSummaryText = string.Empty;

    [ObservableProperty]
    private string _reportSignerText = NoValue;

    [ObservableProperty]
    private string _reportHoursText = NoValue;

    [ObservableProperty]
    private string _reportMeterText = NoValue;

    [ObservableProperty]
    private string _reportSealText = string.Empty;

    [ObservableProperty]
    private bool _sealIsBroken;

    [ObservableProperty]
    private string _billableTotalText = NoValue;

    [ObservableProperty]
    private string _coveredTotalText = NoValue;

    [ObservableProperty]
    private bool _canDownloadPdf;

    [ObservableProperty]
    private bool _canInvoice;

    [ObservableProperty]
    private string _invoiceText = string.Empty;

    [ObservableProperty]
    private string _taxRateText = "22";

    [ObservableProperty]
    private string _partProductId = string.Empty;

    [ObservableProperty]
    private string _partDescription = string.Empty;

    [ObservableProperty]
    private string _partQuantity = "1";

    [ObservableProperty]
    private string _partUnitPrice = "0";

    [ObservableProperty]
    private string _partLot = string.Empty;

    [ObservableProperty]
    private string _signSummary = string.Empty;

    [ObservableProperty]
    private string _signLaborHours = "0";

    [ObservableProperty]
    private string _signTravelHours = "0";

    [ObservableProperty]
    private string _signTravelKm = "0";

    [ObservableProperty]
    private bool _signApplyCallout;

    [ObservableProperty]
    private string _signMeterHours = string.Empty;

    [ObservableProperty]
    private string _signMeterCycles = string.Empty;

    [ObservableProperty]
    private string _signSignerName = string.Empty;

    [ObservableProperty]
    private string _signSignerRole = string.Empty;

    [ObservableProperty]
    private string _signSignatureSvg = string.Empty;

    [ObservableProperty]
    private FieldServiceInkSketch? _signInkSketch;

    [ObservableProperty]
    private int _signInkClearRequest;

    [ObservableProperty]
    private int _signInkUndoRequest;

    [ObservableProperty]
    private string _signSignatureHashText = string.Empty;

    [ObservableProperty]
    private string _signStrokesText = string.Empty;

    [ObservableProperty]
    private FieldServiceChoice<string>? _selectedLocale;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldWorkOrderViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public FieldWorkOrderViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _labels = new FieldServiceLabels(localization);
        LastSyncedText = _labels.Key("FieldService_NeverSynced");
        ShowSignature(null);

        foreach (var result in new[] { "pass", "fail", "na" })
        {
            ChecklistResults.Add(new FieldServiceChoice<string>(result, _labels.Key("FieldService_Check_" + result)));
        }

        foreach (var locale in new[] { "it-IT", "it-CH", "de-CH", "fr-CH", "en" })
        {
            Locales.Add(new FieldServiceChoice<string>(locale, _labels.Key("FieldService_Locale_" + locale)));
        }

        SelectedLocale = Locales.FirstOrDefault(choice => string.Equals(choice.Value, DefaultLocaleOf(localization.CurrentCulture.Name), StringComparison.Ordinal));
    }

    /// <summary>
    /// Occurs when an operation was queued or sent, so the outbox summary refreshes.
    /// </summary>
    public event EventHandler? OutboxChanged;

    /// <summary>
    /// Gets the work orders of the picker.
    /// </summary>
    public ObservableCollection<FieldWorkOrderRow> WorkOrders { get; } = [];

    /// <summary>
    /// Gets the technicians known from the dispatcher board.
    /// </summary>
    public ObservableCollection<FieldServiceChoice<string>> Technicians { get; } = [];

    /// <summary>
    /// Gets the offline work bundle of the selected technician.
    /// </summary>
    public ObservableCollection<FieldServiceBundleRow> Bundle { get; } = [];

    /// <summary>
    /// Gets the parts consumed on the work order, covered and chargeable apart.
    /// </summary>
    public ObservableCollection<FieldServicePartRow> Parts { get; } = [];

    /// <summary>
    /// Gets the lines of the signed service report checklist.
    /// </summary>
    public ObservableCollection<string> ReportChecklist { get; } = [];

    /// <summary>
    /// Gets the priced lines of the service report.
    /// </summary>
    public ObservableCollection<FieldServiceBillingRow> BillingLines { get; } = [];

    /// <summary>
    /// Gets the checklist of the report being signed.
    /// </summary>
    public ObservableCollection<FieldServiceChecklistEditRow> ChecklistEdit { get; } = [];

    /// <summary>
    /// Gets the results a checklist line can have.
    /// </summary>
    public ObservableCollection<FieldServiceChoice<string>> ChecklistResults { get; } = [];

    /// <summary>
    /// Gets the languages of the service report PDF.
    /// </summary>
    public ObservableCollection<FieldServiceChoice<string>> Locales { get; } = [];

    /// <summary>
    /// Loads the work order list, the technicians and the selected work order from the cache, pulling the list when the cache is
    /// empty.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() =>
        GuardAsync(async () =>
        {
            if (!await ShowListAsync())
            {
                await PullAsync(FieldServiceSyncScope.WorkOrders, key: null);
                _ = await ShowListAsync();
            }

            await ShowTechniciansAsync();
            _ = await ShowBundleAsync();
            await ShowSelectedAsync(syncIfMissing: true);
        });

    /// <summary>
    /// Pulls the work order list, the selected work order and the bundle of the selected technician from the server.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public Task SyncAsync() =>
        GuardAsync(async () =>
        {
            await PullAsync(FieldServiceSyncScope.WorkOrders, key: null);
            _ = await ShowListAsync();
            await ShowTechniciansAsync();

            if (SelectedWorkOrder is { } selected)
            {
                await PullAsync(FieldServiceSyncScope.WorkOrder, selected.Id);
            }

            if (SelectedTechnician is { } technician)
            {
                await PullAsync(FieldServiceSyncScope.Bundle, technician.Value);
                _ = await ShowBundleAsync();
            }

            await ShowSelectedAsync(syncIfMissing: false);
        });

    /// <summary>
    /// Reloads the lists and the selected work order from the cache, after the hub changed the underlying data.
    /// </summary>
    /// <returns>A task representing the refresh.</returns>
    public Task RefreshAsync() => LoadAsync();

    private static string DefaultLocaleOf(string cultureName) => cultureName switch
    {
        "de-CH" => "de-CH",
        "fr-CH" => "fr-CH",
        "it-CH" => "it-CH",
        "en-US" or "en" => "en",
        _ => FieldServiceValidation.DefaultLocale,
    };

    private static bool IsAdvanceable(string status) =>
        string.Equals(status, "dispatched", StringComparison.Ordinal) || string.Equals(status, "en_route", StringComparison.Ordinal);

    private static string PartStateKey(FieldServicePart part)
    {
        var settled = part.Covered ? "FieldService_Part_Covered" : "FieldService_Part_Chargeable";

        return part.IsPending ? "FieldService_Part_Pending" : settled;
    }

    partial void OnSelectedWorkOrderChanged(FieldWorkOrderRow? value)
    {
        if (_suppressSelection || value is null)
        {
            return;
        }

        _ = ReloadSelectedAsync();
    }

    partial void OnSelectedTechnicianChanged(FieldServiceChoice<string>? value)
    {
        if (_suppressSelection || value is null)
        {
            return;
        }

        _ = ReloadBundleAsync(value);
    }

    private async Task GuardAsync(Func<Task> action)
    {
        IsBusy = true;

        try
        {
            await action();
        }
        catch (ValidationException ex)
        {
            StatusMessage = ex.Errors.Select(error => _labels.Key(error.ErrorMessage)).FirstOrDefault() ?? ex.Message;
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

    private Task ReloadSelectedAsync() => GuardAsync(() => ShowSelectedAsync(syncIfMissing: true));

    private Task ReloadBundleAsync(FieldServiceChoice<string> technician) => GuardAsync(async () =>
    {
        if (!await ShowBundleAsync())
        {
            await PullAsync(FieldServiceSyncScope.Bundle, technician.Value);
            _ = await ShowBundleAsync();
        }
    });

    private async Task PullAsync(FieldServiceSyncScope scope, string? key)
    {
        var result = await _sender.Send(new SyncFieldServiceCommand(scope, key, From: null, To: null));
        IsOffline = result.Status != FieldServiceCallStatus.Ok;
        StatusMessage = _labels.Call(result.Status, result.Detail);
    }

    private async Task<bool> ShowListAsync()
    {
        var cached = await _sender.Send(new GetFieldWorkOrdersQuery());
        var keep = SelectedWorkOrder?.Id;

        _suppressSelection = true;
        try
        {
            WorkOrders.Clear();
            foreach (var order in cached?.Value ?? [])
            {
                WorkOrders.Add(new FieldWorkOrderRow(order.Id, order.Number, order.Title, _labels.Status(order.Status), _labels.WorkType(order.WorkType), _labels.Window(order.Start, order.End)));
            }

            SelectedWorkOrder = WorkOrders.FirstOrDefault(row => string.Equals(row.Id, keep, StringComparison.Ordinal)) ?? WorkOrders.FirstOrDefault();
        }
        finally
        {
            _suppressSelection = false;
        }

        return WorkOrders.Count > 0;
    }

    private async Task ShowTechniciansAsync()
    {
        var board = await _sender.Send(new GetDispatcherGanttAndMapBoardQuery());
        var keep = SelectedTechnician?.Value;

        _suppressSelection = true;
        try
        {
            Technicians.Clear();
            foreach (var lane in board?.Value.Lanes ?? [])
            {
                Technicians.Add(new FieldServiceChoice<string>(lane.TechnicianId, lane.Name));
            }

            SelectedTechnician = Technicians.FirstOrDefault(choice => string.Equals(choice.Value, keep, StringComparison.Ordinal));
        }
        finally
        {
            _suppressSelection = false;
        }
    }

    private async Task<bool> ShowBundleAsync()
    {
        Bundle.Clear();
        if (SelectedTechnician is not { } technician)
        {
            return false;
        }

        var cached = await _sender.Send(new GetTechnicianOfflineWorkQueueQuery(technician.Value));
        foreach (var item in cached?.Value.Items ?? [])
        {
            Bundle.Add(new FieldServiceBundleRow(
                item.Id,
                item.Number,
                item.Title,
                _labels.Status(item.Status),
                _labels.Window(item.Start, item.End),
                item.CustomerName ?? NoValue,
                item.EquipmentSerial ?? NoValue,
                string.Join(", ", item.Checklist.Select(line => _labels.Key("FieldService_Check_" + line)))));
        }

        return cached is not null;
    }

    private async Task ShowSelectedAsync(bool syncIfMissing)
    {
        if (SelectedWorkOrder is not { } selected)
        {
            Apply(cached: null);

            return;
        }

        var cached = await _sender.Send(new GetFieldWorkOrderQuery(selected.Id));
        if (cached is null && syncIfMissing)
        {
            await PullAsync(FieldServiceSyncScope.WorkOrder, selected.Id);
            cached = await _sender.Send(new GetFieldWorkOrderQuery(selected.Id));
        }

        Apply(cached);
    }

    private void Apply(FieldServiceCached<FieldServiceWorkOrder>? cached)
    {
        _order = cached?.Value;
        HasOrder = cached is not null;
        Parts.Clear();
        BillingLines.Clear();
        ReportChecklist.Clear();

        if (cached is null)
        {
            ShowHeader(order: null);
            ShowReport(report: null);
            LastSyncedText = _labels.Key("FieldService_NeverSynced");

            return;
        }

        var order = cached.Value;
        LastSyncedText = _labels.Format("FieldService_LastSynced", cached.SyncedAtUtc.ToLocalTime().ToString("g", _labels.Culture));
        ShowHeader(order);
        ShowParts(order);
        ShowReport(order.Report);
        PrepareSigning(order);
    }

    private void ShowHeader(FieldServiceWorkOrder? order)
    {
        NumberText = order?.Number ?? NoValue;
        TitleText = order?.Title ?? NoValue;
        StatusText = order is null ? NoValue : _labels.Status(order.Status);
        TypeText = order is null ? NoValue : _labels.WorkType(order.WorkType);
        PriorityText = order is null ? NoValue : _labels.Priority(order.Priority);
        ProblemText = order?.Problem ?? string.Empty;
        WindowText = order is null ? NoValue : _labels.Window(order.Start, order.End);
        SiteText = order?.Address ?? NoValue;
        SkillsText = order is null || order.RequiredSkills.Count == 0 ? NoValue : string.Join(", ", order.RequiredSkills);
        CanAdvance = order is not null && IsAdvanceable(order.Status);
        CanSign = order is not null && order.Report is null && order.Status is not ("completed" or "invoiced" or "cancelled");
        HasPendingOperations = order is not null && (order.Parts.Any(part => part.IsPending) || order.Report?.IsPending == true);
    }

    private void ShowParts(FieldServiceWorkOrder order)
    {
        foreach (var part in order.Parts)
        {
            Parts.Add(new FieldServicePartRow(
                part.Description,
                _labels.Number(part.Quantity),
                part.IsPending ? NoValue : _labels.Number(part.UnitPrice),
                _labels.Key(PartStateKey(part)),
                part.Covered,
                part.IsPending));
        }
    }

    private void ShowReport(FieldServiceReport? report)
    {
        HasReport = report is not null;
        ReportIsPending = report?.IsPending ?? false;

        if (report is null)
        {
            ReportNumberText = NoValue;
            ReportSummaryText = string.Empty;
            ReportSignerText = NoValue;
            ReportHoursText = NoValue;
            ReportMeterText = NoValue;
            ReportSealText = string.Empty;
            SealIsBroken = false;
            BillableTotalText = NoValue;
            CoveredTotalText = NoValue;
            CanDownloadPdf = false;
            CanInvoice = false;
            InvoiceText = string.Empty;

            return;
        }

        ReportNumberText = report.IsPending ? _labels.Key("FieldService_Report_Pending") : report.Number;
        ReportSummaryText = report.Summary;
        ReportSignerText = _labels.Format("FieldService_Report_Signer", report.SignerName, report.SignerRole ?? NoValue, _labels.Moment(report.SignedAt));
        ReportHoursText = _labels.Format("FieldService_Report_Hours", _labels.Number(report.LaborHours), _labels.Number(report.TravelHours), _labels.Number(report.TravelKm));
        ReportMeterText = _labels.Format("FieldService_Report_Meter", report.MeterHours is { } hours ? _labels.Number(hours) : NoValue, report.MeterCycles?.ToString(_labels.Culture) ?? NoValue);
        SealIsBroken = !report.IsPending && !report.SignatureIntact;
        var sealKey = report.SignatureIntact ? "FieldService_Report_SealOk" : "FieldService_Report_SealBroken";
        ReportSealText = report.IsPending ? string.Empty : _labels.Key(sealKey);
        BillableTotalText = report.IsPending ? NoValue : _labels.Money(report.Billing.BillableTotal, report.Billing.Currency);
        CoveredTotalText = report.IsPending ? NoValue : _labels.Money(report.Billing.CoveredTotal, report.Billing.Currency);
        CanDownloadPdf = !report.IsPending && report.HasPdf;
        CanInvoice = !report.IsPending && report.InvoiceId is null && report.Billing.BillableTotal > 0;
        InvoiceText = report.InvoiceId is null ? string.Empty : _labels.Format("FieldService_Report_Invoiced", report.InvoiceId);

        foreach (var line in report.Checklist)
        {
            ReportChecklist.Add(_labels.Key("FieldService_Check_" + line.Item) + ": " + _labels.Key("FieldService_Check_" + line.Result) + (string.IsNullOrEmpty(line.Notes) ? string.Empty : " (" + line.Notes + ")"));
        }

        foreach (var line in report.Billing.Lines)
        {
            BillingLines.Add(new FieldServiceBillingRow(
                _labels.BillingKind(line.Kind),
                line.Description,
                _labels.Number(line.Quantity),
                _labels.Money(line.ListAmount, report.Billing.Currency),
                _labels.Money(line.ChargedAmount, report.Billing.Currency),
                _labels.Key(line.Covered ? "FieldService_Part_Covered" : "FieldService_Part_Chargeable"),
                line.Covered));
        }
    }

    private void PrepareSigning(FieldServiceWorkOrder order)
    {
        if (string.Equals(_formOrderId, order.Id, StringComparison.Ordinal))
        {
            return;
        }

        // A new work order starts with empty forms and its own checklist.
        _formOrderId = order.Id;
        SignSummary = string.Empty;
        SignLaborHours = "0";
        SignTravelHours = "0";
        SignTravelKm = "0";
        SignApplyCallout = false;
        SignMeterHours = string.Empty;
        SignMeterCycles = string.Empty;
        ClearSignature();
        PartProductId = string.Empty;
        PartDescription = string.Empty;
        PartQuantity = "1";
        PartUnitPrice = "0";
        PartLot = string.Empty;

        ChecklistEdit.Clear();
        foreach (var item in FieldServiceChecklistTemplates.For(order.WorkType))
        {
            ChecklistEdit.Add(new FieldServiceChecklistEditRow(item, _labels.Key("FieldService_Check_" + item)));
        }
    }

    private void RaiseOutboxChanged() => OutboxChanged?.Invoke(this, EventArgs.Empty);
}
