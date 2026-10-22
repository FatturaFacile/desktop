// <copyright file="InvoicesViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Manages billing presentations, querying active sales invoices across both Italian SDI (FatturaPA XML) and Swiss QR-Bill (EPC/QR-bill SVG) formats, and coordinating document previews.
/// </summary>
/// <param name="mediator">The MediatR sender used to execute invoice queries.</param>
/// <param name="invoiceDocumentService">An optional service for generating FatturaPA XML and Swiss QR-bill SVG representations.</param>
public sealed partial class InvoicesViewModel(ISender mediator, IInvoiceDocumentService? invoiceDocumentService = null) : ViewModelBase
{
    private readonly ISender mediator = mediator;
    private readonly IInvoiceDocumentService? invoiceDocumentService = invoiceDocumentService;

    /// <summary>
    /// Gets or sets the collection of retrieved Italian electronic invoices.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ItalianInvoiceDto> Invoices { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of retrieved Swiss QR-bill records.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<SwissQrBillDto> SwissQrBills { get; set; } = [];

    /// <summary>
    /// Gets or sets the currently selected Italian invoice in the grid or preview panel, or <see langword="null"/> if none is selected.
    /// </summary>
    [ObservableProperty]
    public partial ItalianInvoiceDto? SelectedInvoice { get; set; }

    /// <summary>
    /// Gets or sets the currently selected Swiss QR-bill in the grid or preview panel, or <see langword="null"/> if none is selected.
    /// </summary>
    [ObservableProperty]
    public partial SwissQrBillDto? SelectedSwissQrBill { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document preview panel/flyout is currently displayed.
    /// </summary>
    [ObservableProperty]
    public partial bool IsPreviewOpen { get; set; }

    /// <summary>
    /// Gets or sets the raw XML content generated for Italian SDI FatturaPA previews.
    /// </summary>
    [ObservableProperty]
    public partial string PreviewXml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vector SVG markup generated for Swiss QR payment slip previews.
    /// </summary>
    [ObservableProperty]
    public partial string PreviewSvg { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the Swiss QR-bill tab is currently selected in the user interface.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSwissTabSelected { get; set; }

    /// <summary>
    /// Asynchronously initializes the ViewModel by loading invoices and bills across jurisdictions.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
    public override Task InitializeAsync() => LoadDataAsync();

    /// <summary>
    /// Dispatches a <see cref="GetInvoicesOverviewQuery"/> via MediatR to retrieve both Italian invoices and Swiss QR bills.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous data loading operation.</returns>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var overview = await mediator.Send(new GetInvoicesOverviewQuery());
            Invoices = [..overview.ItalianInvoices];
            SwissQrBills = [..overview.SwissQrBills];

            SelectedInvoice = Invoices.FirstOrDefault();
            SelectedSwissQrBill = SwissQrBills.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Generates and previews standard SDI FatturaPA XML content for the specified Italian electronic invoice.
    /// </summary>
    /// <param name="invoice">The Italian invoice DTO whose XML is to be generated and displayed.</param>
    [RelayCommand]
    public void ViewFatturaPaXml(
        ItalianInvoiceDto invoice)
    {
        if (invoice == null)
        {
            return;
        }

        SelectedInvoice = invoice;

        // Costruisce l'aggregato per generare l'XML nativo
        var dummyInvoice = ItalianElectronicInvoice.Create(
            invoice.TenantId,
            invoice.DocumentNumber,
            invoice.DocumentDate,
            invoice.ClientId,
            invoice.CedenteDenominazione,
            invoice.CedentePartitaIva,
            invoice.CedentePartitaIva,
            new Address(
                "Via Monte Napoleone 8",
                "Milano",
                "20121",
                "MI",
                "IT"),
            invoice.CessionarioDenominazione,
            invoice.CessionarioPartitaIva,
            cessionarioCf: null,
            invoice.CessionarioCodiceDestinatario,
            pec: null,
            new Address(
                "Via delle Industrie 45",
                "Monza",
                "20900",
                "MB",
                "IT"),
            invoice.PaymentDueDate);

        foreach (var line in invoice.Lines)
        {
            dummyInvoice.AddLine(
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.VatRate,
                line.NaturaExemptionCode);
        }

        PreviewXml = invoiceDocumentService?.GenerateFatturaPaXml(dummyInvoice) ?? string.Empty;
        PreviewSvg = string.Empty;
        IsPreviewOpen = true;
    }

    /// <summary>
    /// Generates and previews the Swiss QR-bill SVG slip conforming to the PaymentStandards.CH specification.
    /// </summary>
    /// <param name="bill">The Swiss QR-bill DTO whose payment slip SVG is to be rendered.</param>
    [RelayCommand]
    public void ViewSwissQrSlip(
        SwissQrBillDto bill)
    {
        if (bill == null)
        {
            return;
        }

        SelectedSwissQrBill = bill;

        var dummyBill = SwissQrBill.Create(
            bill.TenantId,
            bill.BillNumber,
            bill.BillDate,
            bill.ClientId,
            bill.CreditorName,
            new Address(
                "Via Nassa 12",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            new QrIban(bill.CreditorQrIban),
            bill.DebtorName,
            new Address(
                "Via San Gottardo 102",
                "Bellinzona",
                "6500",
                "TI",
                "CH"),
            Money.Chf(bill.Amount),
            bill.ReferenceNumber,
            bill.DueDate,
            bill.AdditionalMessage);

        PreviewSvg = invoiceDocumentService?.GenerateSwissQrSvg(dummyBill) ?? string.Empty;
        PreviewXml = string.Empty;
        IsPreviewOpen = true;
    }

    /// <summary>
    /// Dismisses the preview overlay and clears existing XML and SVG preview buffers.
    /// </summary>
    [RelayCommand]
    public void ClosePreview()
    {
        IsPreviewOpen = false;
        PreviewXml = string.Empty;
        PreviewSvg = string.Empty;
    }
}
