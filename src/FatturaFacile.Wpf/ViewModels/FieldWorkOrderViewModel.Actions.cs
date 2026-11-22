// <copyright file="FieldWorkOrderViewModel.Actions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The write actions of the work order view: the technician operations (queued offline) and the online report actions.
/// </summary>
internal sealed partial class FieldWorkOrderViewModel
{
    /// <summary>
    /// Moves the work order to en route.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public Task EnRouteAsync() => AdvanceAsync("en_route");

    /// <summary>
    /// Moves the work order to on site.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public Task OnSiteAsync() => AdvanceAsync("on_site");

    /// <summary>
    /// Records a part taken from the van. It is saved locally first and sent with an idempotency key, so it works offline.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public async Task RecordPartAsync()
    {
        if (SelectedWorkOrder is not { } selected || !Guid.TryParse(selected.Id, out var workOrderId))
        {
            return;
        }

        if (!Guid.TryParse(PartProductId, out var productId))
        {
            StatusMessage = _labels.Key("FieldService_Err_Product");

            return;
        }

        if (!TryAmount(PartQuantity, out var quantity) || !TryAmount(PartUnitPrice, out var unitPrice))
        {
            StatusMessage = _labels.Key("FieldService_Err_Number");

            return;
        }

        var description = string.IsNullOrWhiteSpace(PartDescription) ? PartProductId : PartDescription.Trim();
        var part = new FieldServicePartInput(workOrderId, productId, description, quantity, unitPrice, string.IsNullOrWhiteSpace(PartLot) ? null : PartLot.Trim());

        await GuardAsync(async () => await AfterQueueAsync(await _sender.Send(new RecordVanStockPartConsumptionCommand(part)), selected.Id));
    }

    /// <summary>
    /// Signs and closes the service report: the checklist, the labor and travel metrics, the meter readings and the customer
    /// signature. It is saved locally first, so a technician offline can still close the visit.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public async Task SignAndCloseAsync()
    {
        if (SelectedWorkOrder is not { } selected || !Guid.TryParse(selected.Id, out var workOrderId))
        {
            return;
        }

        if (BuildReport(workOrderId) is not { } report)
        {
            StatusMessage = _labels.Key("FieldService_Err_Number");

            return;
        }

        if (!FieldServiceValidation.IsSafeSvg(report.SignatureSvg))
        {
            StatusMessage = _labels.Key(FieldServiceValidation.SignatureKey);

            return;
        }

        await GuardAsync(async () => await AfterQueueAsync(await _sender.Send(new SignAndCloseServiceReportCommand(report)), selected.Id));
    }

    /// <summary>
    /// Clears the signature pad: the strokes, the SVG and its hash.
    /// </summary>
    [RelayCommand]
    public void ClearSignature()
    {
        SignInkClearRequest++;
        SignInkSketch = null;
        ShowSignature(null);
    }

    /// <summary>
    /// Takes back the last stroke of the signature pad.
    /// </summary>
    [RelayCommand]
    public void UndoSignature()
    {
        if (SignInkSketch is not { Strokes.Count: > 0 } sketch)
        {
            return;
        }

        SignInkUndoRequest++;

        // The pad publishes the new sketch itself; this keeps the model right when it is not attached to a view.
        SignInkSketch = sketch with { Strokes = [.. sketch.Strokes.Take(sketch.Strokes.Count - 1)] };
    }

    /// <summary>
    /// Downloads the signed service report as PDF from the server and saves it in the service report folder.
    /// </summary>
    /// <returns>A task representing the download.</returns>
    [RelayCommand]
    public async Task DownloadPdfAsync()
    {
        if (_order?.Report is not { IsPending: false } report)
        {
            return;
        }

        await GuardAsync(async () =>
        {
            var result = await _sender.Send(new DownloadServiceReportPdfCommand(report.Id));
            IsOffline = result.Status == FieldServiceCallStatus.Offline;
            StatusMessage = result.Status == FieldServiceCallStatus.Ok
                ? _labels.Format("FieldService_PdfSaved", result.Path ?? string.Empty)
                : _labels.Call(result.Status, result.Detail);
        });
    }

    /// <summary>
    /// Creates the draft invoice of the chargeable part of the service report; the lines covered by the contract are not billed.
    /// </summary>
    /// <returns>A task representing the conversion.</returns>
    [RelayCommand]
    public async Task ConvertToInvoiceAsync()
    {
        if (_order?.Report is not { IsPending: false } report || SelectedWorkOrder is not { } selected)
        {
            return;
        }

        if (!TryAmount(TaxRateText, out var taxRate))
        {
            StatusMessage = _labels.Key("FieldService_Err_TaxRate");

            return;
        }

        await GuardAsync(async () =>
        {
            var result = await _sender.Send(new ConvertServiceReportToInvoiceCommand(report.Id, taxRate, DueDate: null));
            IsOffline = result.Status == FieldServiceCallStatus.Offline;

            if (result.Status == FieldServiceCallStatus.Ok)
            {
                await PullAsync(FieldServiceSyncScope.WorkOrder, selected.Id);
                await ShowSelectedAsync(syncIfMissing: false);
                StatusMessage = _labels.Format("FieldService_InvoiceCreated", result.Id ?? string.Empty);
            }
            else
            {
                StatusMessage = _labels.Call(result.Status, result.Detail);
            }
        });
    }

    partial void OnSignInkSketchChanged(FieldServiceInkSketch? value) => ShowSignature(value);

    private void ShowSignature(FieldServiceInkSketch? sketch)
    {
        var svg = sketch is null ? null : FieldServiceSignatureSvg.Build(sketch);

        SignSignatureSvg = svg ?? string.Empty;
        SignSignatureHashText = svg is null
            ? _labels.Key("FieldService_Signature_Empty")
            : _labels.Format("FieldService_Signature_Hash", FieldServiceSignatureSvg.Sha256Hex(svg));
        SignStrokesText = _labels.Format("FieldService_Signature_Strokes", FieldServiceSignatureSvg.CountStrokes(sketch).ToString(_labels.Culture));
    }

    private bool TryAmount(string text, out decimal value) => FieldServiceNumbers.TryAmount(text, _labels.Culture, out value);

    private FieldServiceReportInput? BuildReport(Guid workOrderId)
    {
        if (!TryAmount(SignLaborHours, out var labor)
            || !TryAmount(SignTravelHours, out var travelHours)
            || !TryAmount(SignTravelKm, out var travelKm)
            || !FieldServiceNumbers.TryOptional(SignMeterHours, _labels.Culture, out var meterHours)
            || !FieldServiceNumbers.TryWhole(SignMeterCycles, _labels.Culture, out var meterCycles))
        {
            return null;
        }

        var checklist = ChecklistEdit
            .Select(row => new FieldServiceChecklistItem(row.Item, row.Result, string.IsNullOrWhiteSpace(row.Notes) ? null : row.Notes.Trim()))
            .ToList();

        return new FieldServiceReportInput(
            workOrderId,
            SignSummary.Trim(),
            checklist,
            labor,
            travelHours,
            travelKm,
            SignApplyCallout,
            meterHours,
            meterCycles,
            SignSignerName.Trim(),
            string.IsNullOrWhiteSpace(SignSignerRole) ? null : SignSignerRole.Trim(),
            SignSignatureSvg,
            SelectedLocale?.Value ?? FieldServiceValidation.DefaultLocale);
    }

    private Task AdvanceAsync(string status)
    {
        if (SelectedWorkOrder is not { } selected || !Guid.TryParse(selected.Id, out var workOrderId))
        {
            return Task.CompletedTask;
        }

        return GuardAsync(async () => await AfterQueueAsync(await _sender.Send(new AdvanceFieldWorkOrderStatusCommand(workOrderId, status)), selected.Id));
    }

    private async Task AfterQueueAsync(FieldServiceQueueResult result, string workOrderId)
    {
        if (result.ErrorKey is null && result.Sent)
        {
            // The server applied it: pull the work order so the parts come back priced and the report sealed.
            await PullAsync(FieldServiceSyncScope.WorkOrder, workOrderId);
        }

        StatusMessage = QueueMessage(result);
        IsOffline = result.ErrorKey is null && !result.Sent && !result.Conflict;

        if (result.ErrorKey is not null)
        {
            return;
        }

        // The forms start again for the next operation.
        _formOrderId = null;
        await ShowSelectedAsync(syncIfMissing: false);
        RaiseOutboxChanged();
    }

    private string QueueMessage(FieldServiceQueueResult result) => (result.ErrorKey, result.Sent, result.Conflict) switch
    {
        ({ } key, _, _) => _labels.Key(key),
        (_, _, true) => _labels.Key("FieldService_QueueConflict"),
        (_, true, _) => _labels.Key("FieldService_QueueSent"),
        _ => _labels.Key("FieldService_QueueQueued"),
    };
}
